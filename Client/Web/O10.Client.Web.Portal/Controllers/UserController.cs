using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.Services;
using O10.Client.Web.Portal.Services;
using O10.Core.ExtensionMethods;
using O10.Client.DataLayer.Enums;
using O10.Client.Web.Portal.Dtos;
using O10.Core.Configuration;
using O10.Crypto.ConfidentialAssets;
using System.Text;
using O10.Client.Common.Identities;
using O10.Client.Common.Interfaces.Inputs;
using System.Globalization;
using Flurl;
using Flurl.Http;
using System.Net.Http;
using O10.Client.DataLayer.Model;
using System.Collections.Specialized;
using O10.Client.Common.Interfaces.Outputs;
using O10.Client.Web.Portal.Dtos.User;
using Microsoft.AspNetCore.SignalR;
using System.Web;
using O10.Client.Web.Common.Dtos.SamlIdp;
using O10.Client.Web.Common.Services;
using O10.Client.Web.Common.Hubs;
using O10.Client.Web.Common.Dtos.Biometric;
using O10.Core.Cryptography;
using System.Threading.Tasks;
using O10.Transactions.Core.Ledgers.Stealth.Internal;
using O10.Client.DataLayer.AttributesScheme;
using O10.Client.Common.Configuration;
using System.Threading;
using O10.Core;
using O10.Core.Logging;
using Newtonsoft.Json;
using O10.Core.HashCalculations;
using O10.Client.Common.Entities;
using O10.Client.Web.Portal.Configuration;
using O10.Core.Identity;
using O10.Client.Common.Dtos.UniversalProofs;
using O10.Client.Common.Communication;
using O10.Client.Web.Portal.Exceptions;
using O10.Client.Web.Portal.ElectionCommittee.Models;
using Flurl.Util;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using O10.Core.Serialization;
using O10.Transactions.Core.DTOs;

namespace O10.Client.Web.Portal.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IDocumentSignatureVerifier _documentSignatureVerifier;
        private readonly IAccountsService _accountsService;
        private readonly IAssetsService _assetsService;
        private readonly IExecutionContextManager _executionContextManager;
        private readonly IIdentityAttributesService _identityAttributesService;
        private readonly IDataAccessService _dataAccessService;
        private readonly IGatewayService _gatewayService;
        private readonly ISchemeResolverService _schemeResolverService;
        private readonly IEligibilityProofsProvider _eligibilityProofsProvider;
        private readonly IStealthClientCryptoService _stealthClientCryptoService;
        private readonly IIdentityKeyProvider _identityKeyProvider;
        private readonly IHubContext<IdentitiesHub> _idenitiesHubContext;
        private readonly IRestApiConfiguration _restApiConfiguration;
        private readonly IHashCalculation _hashCalculation;
        private readonly ILogger _logger;
        private readonly IPortalConfiguration _portalConfiguration;

        public UserController(IDocumentSignatureVerifier documentSignatureVerifier,
                              IAccountsService accountsService,
                              IAssetsService assetsService,
                              IExecutionContextManager executionContextManager,
                              IIdentityAttributesService identityAttributesService,
                              IDataAccessService externalDataAccessService,
                              IGatewayService gatewayService,
                              ISchemeResolverService schemeResolverService,
                              IIdentityKeyProvidersRegistry identityKeyProvidersRegistry,
                              IEligibilityProofsProvider eligibilityProofsProvider,
                              IStealthClientCryptoService stealthClientCryptoService,
                              IConfigurationService configurationService,
                              IHubContext<IdentitiesHub> idenitiesHubContext,
                              ILoggerService loggerService,
                              IHashCalculationsRepository hashCalculationsRepository)
        {
            _documentSignatureVerifier = documentSignatureVerifier;
            _accountsService = accountsService;
            _assetsService = assetsService;
            _executionContextManager = executionContextManager;
            _identityAttributesService = identityAttributesService;
            _dataAccessService = externalDataAccessService;
            _gatewayService = gatewayService;
            _schemeResolverService = schemeResolverService;
            _eligibilityProofsProvider = eligibilityProofsProvider;
            _stealthClientCryptoService = stealthClientCryptoService;
            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
            _idenitiesHubContext = idenitiesHubContext;
            _restApiConfiguration = configurationService.Get<IRestApiConfiguration>();
            _portalConfiguration = configurationService.Get<IPortalConfiguration>();
            _hashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
            _logger = loggerService.GetLogger(nameof(UserController));
        }

        [HttpGet("{accountId}/Attributes")]
        public async Task<ActionResult<IEnumerable<UserAttributeSchemeDto>>> GetUserAttributes(long accountId)
        {
            var scope = _executionContextManager.ResolveExecutionServices(accountId).Scope;
            var assetsService = scope.ServiceProvider.GetService<IAssetsService>();
            IEnumerable<UserRootAttribute> userRootAttributes = _dataAccessService.GetUserAttributes(accountId);
            List<UserAttributeSchemeDto> userAttributeSchemes = new List<UserAttributeSchemeDto>();

            foreach (var rootAttribute in userRootAttributes)
            {
                var issuer = rootAttribute.Source;
                var userAttributeScheme = userAttributeSchemes.Find(i => i.Issuer == issuer && i.RootAssetId == rootAttribute.AssetId.ToHexString());
                if(userAttributeScheme == null)
                {
                    userAttributeScheme = new UserAttributeSchemeDto
                    {
                        Issuer = issuer,
                        IssuerName = _dataAccessService.GetUserIdentityIsserAlias(issuer),
                        RootAttributeContent = rootAttribute.Content,
                        RootAssetId = rootAttribute.AssetId.ToHexString(),
                        SchemeName = rootAttribute.SchemeName
                    };

                    userAttributeSchemes.Add(userAttributeScheme);

                    if (string.IsNullOrEmpty(userAttributeScheme.IssuerName))
                    {
                        userAttributeScheme.IssuerName = await ResolveIssuerName(issuer).ConfigureAwait(false);
                    }
                    userAttributeScheme.RootAttributes.Add(await GetUserAttributeDto(rootAttribute).ConfigureAwait(false));

                    var associatedAttributesGrouped = _dataAccessService.GetUserAssociatedAttributes(accountId).Where(a => a.RootAssetId.Equals32(rootAttribute.AssetId)) .GroupBy(a => a.Source);

                    foreach (var group in associatedAttributesGrouped)
                    {
                        string associatedIssuer = group.Key;
                        string issuerName = await ResolveIssuerName(associatedIssuer).ConfigureAwait(false);
                        var rootAttributeDefinition = await assetsService.GetRootAttributeDefinition(associatedIssuer).ConfigureAwait(false);
                        var attributeDefinitions = await assetsService.GetAssociatedAttributeDefinitions(associatedIssuer).ConfigureAwait(false);

                        var userAssociatedAttributes = new UserAssociatedAttributesDto
                        {
                            Issuer = group.Key,
                            IssuerName = issuerName
                        };

                        userAttributeScheme.AssociatedSchemes.Add(userAssociatedAttributes);
                        foreach (var associatedAttribute in group)
                        {
                            userAssociatedAttributes.Attributes.Add(new UserAssociatedAttributeDto
                            {
                                Alias = (rootAttributeDefinition.AttributeName == associatedAttribute.AttributeSchemeName ? rootAttributeDefinition : attributeDefinitions.FirstOrDefault(a => a.AttributeName == associatedAttribute.AttributeSchemeName))?.Alias,
                                SchemeName = associatedAttribute.AttributeSchemeName,
                                Content = associatedAttribute.Content,
                                AttributeId = associatedAttribute.UserAssociatedAttributeId
                            });
                        }
                    }

                }
            }

            foreach (var attributeScheme in userAttributeSchemes)
            {
                SetIdentitySchemeState(attributeScheme);
            }

            return userAttributeSchemes;
        }

        private async Task<string> ResolveIssuerName(string issuer)
        {
            string issuerName = null;
            await _schemeResolverService.ResolveIssuer(issuer)
                .ContinueWith(t =>
                {
                    if (t.IsCompletedSuccessfully)
                    {
                        _dataAccessService.AddOrUpdateUserIdentityIsser(issuer, t.Result, string.Empty);
                        issuerName = t.Result;
                    }
                    else
                    {
                        issuerName = issuer;
                    }
                }, TaskScheduler.Default).ConfigureAwait(false);

            return issuerName;
        }

        private static void SetIdentitySchemeState(UserAttributeSchemeDto attributeScheme)
        {
            attributeScheme.State = AttributeState.NotConfirmed;

            foreach (var rootAttribute in attributeScheme.RootAttributes)
            {
                if (rootAttribute.State == AttributeState.Confirmed)
                {
                    attributeScheme.State = AttributeState.Confirmed;
                }
                else if (rootAttribute.State == AttributeState.Disabled && attributeScheme.State != AttributeState.Confirmed)
                {
                    attributeScheme.State = AttributeState.Disabled;
                }
            }
        }

        [HttpDelete("UserRootAttribute")]
        public IActionResult DeleteUserRootAttribute(long accountId, long attributeId)
        {
            return Ok(_dataAccessService.RemoveUserAttribute(accountId, attributeId));
        }

        private async Task<UserAttributeDto> GetUserAttributeDto(UserRootAttribute c)
        {
            string issuerName = await _schemeResolverService.ResolveIssuer(c.Source).ConfigureAwait(false);
            return new UserAttributeDto
            {
                UserAttributeId = c.UserAttributeId,
                SchemeName = c.SchemeName,
                Content = c.Content,
                Validated = !string.IsNullOrEmpty(c.Content),
                Source = c.Source,
                IssuerName = issuerName,
                IsOverriden = c.IsOverriden,
                State = c.IsOverriden ? AttributeState.Disabled : (c.LastCommitment.ToHexString() == "0000000000000000000000000000000000000000000000000000000000000000" ? AttributeState.NotConfirmed : AttributeState.Confirmed)
            };
        }

        [HttpPost("CompromisedProofs")]
        public async Task<IActionResult> SendCompromisedProofs(long accountId, [FromBody] UnauthorizedUseDto unauthorizedUse)
        {
            var userSettings = _dataAccessService.GetUserSettings(accountId);
            _logger.LogIfDebug(() => $"[{accountId}]: {nameof(SendCompromisedProofs)}, userSettings={(userSettings != null ? JsonConvert.SerializeObject(userSettings, new ByteArrayJsonConverter()) : "NULL")}");

            if (userSettings?.IsAutoTheftProtection == false)
            {
                _logger.Info("Sending compromised proofs abandoned");
                return Ok();
            }

            _logger.LogIfDebug(() => $"[{accountId}]: {nameof(SendCompromisedProofs)}, unauthorizedUse={(unauthorizedUse != null ? JsonConvert.SerializeObject(unauthorizedUse, new ByteArrayJsonConverter()) : "NULL")}");

            var persistency = _executionContextManager.ResolveExecutionServices(accountId);
            var transactionsService = persistency.Scope.ServiceProvider.GetService<IStealthTransactionsService>();
            UserRootAttribute rootAttribute = null;
            byte[] keyImageCompromized = unauthorizedUse.KeyImage.ToByteArray();
            byte[] transactionKeyCompromized = unauthorizedUse.TransactionKey.ToByteArray();
            byte[] destinationKeyCompromized = unauthorizedUse.DestinationKey.ToByteArray();

            rootAttribute = GetRootAttributeOnTransactionKeyArriving(accountId, transactionKeyCompromized);

            if (rootAttribute == null)
            {
                return BadRequest();
            }

            byte[] target = unauthorizedUse.Target.ToByteArray();

            GetRequestInput(rootAttribute, target, out byte[] issuer, out RequestInput requestInput);

            OutputSources[] outputModels = await _gatewayService.GetOutputs(_restApiConfiguration.RingSize + 1).ConfigureAwait(false);
            byte[][] issuanceCommitments = await _gatewayService.GetIssuanceCommitments(issuer, _restApiConfiguration.RingSize + 1).ConfigureAwait(false);
            RequestResult requestResult = transactionsService.SendCompromisedProofs(requestInput, keyImageCompromized, transactionKeyCompromized, destinationKeyCompromized, outputModels, issuanceCommitments).Result;

            rootAttribute = GetRootAttributeOnTransactionKeyArriving(accountId, requestResult.NewTransactionKey);

            IEnumerable<UserRootAttribute> userRootAttributes = _dataAccessService.GetUserAttributes(accountId).Where(u => !u.IsOverriden);

            foreach (UserRootAttribute userAttribute in userRootAttributes)
            {
                await SendRevokeIdentity(userAttribute, transactionsService).ConfigureAwait(false);
            }

            return Ok();
        }

        private static void GetRequestInput(UserRootAttribute rootAttribute, byte[] target, out byte[] issuer, out RequestInput requestInput)
        {
            issuer = rootAttribute.Source.HexStringToByteArray();
            byte[] assetId = rootAttribute.AssetId;
            byte[] originalBlindingFactor = rootAttribute.OriginalBlindingFactor;
            byte[] originalCommitment = rootAttribute.IssuanceCommitment;
            byte[] lastTransactionKey = rootAttribute.LastTransactionKey;
            byte[] lastBlindingFactor = rootAttribute.LastBlindingFactor;
            byte[] lastCommitment = rootAttribute.LastCommitment;
            byte[] lastDestinationKey = rootAttribute.LastDestinationKey;

            requestInput = new RequestInput
            {
                AssetId = assetId,
                EligibilityBlindingFactor = originalBlindingFactor,
                EligibilityCommitment = originalCommitment,
                Issuer = issuer,
                PrevAssetCommitment = lastCommitment,
                PrevBlindingFactor = lastBlindingFactor,
                PrevDestinationKey = lastDestinationKey,
                PrevTransactionKey = lastTransactionKey,
                PublicSpendKey = target
            };
        }

        private UserRootAttribute GetRootAttributeOnTransactionKeyArriving(long accountId, Memory<byte> transactionKey)
        {
            UserRootAttribute rootAttribute;
            int counter = 0;
            do
            {
                IEnumerable<UserRootAttribute> userAttributes = _dataAccessService.GetUserAttributes(accountId).Where(u => !u.IsOverriden && !u.LastCommitment.Equals32(new byte[32]));
                rootAttribute = userAttributes.FirstOrDefault(a => transactionKey.Equals32(a.LastTransactionKey));

                if (rootAttribute == null)
                {
                    counter++;
                    Thread.Sleep(500);
                }
            } while (rootAttribute == null && counter <= 10);
            return rootAttribute;
        }

        private UserRootAttribute GetRootAttributeOnTransactionKeyChanging(long accountId, byte[] originalCommitment, byte[] transactionKey)
        {
            UserRootAttribute rootAttribute;
            int counter = 0;
            do
            {
                Thread.Sleep(500);
                rootAttribute = _dataAccessService.GetUserAttributes(accountId).FirstOrDefault(u => !u.IsOverriden && u.IssuanceCommitment.Equals32(originalCommitment) && !u.LastTransactionKey.Equals32(transactionKey));
            } while (rootAttribute == null && counter <= 10);
            return rootAttribute;
        }

        private async Task SendRevokeIdentity(UserRootAttribute rootAttribute, IStealthTransactionsService transactionsService)
        {
            byte[] target = rootAttribute.Source.HexStringToByteArray();
            byte[] issuer = rootAttribute.Source.HexStringToByteArray();
            byte[] assetId = rootAttribute.AssetId;
            byte[] originalBlindingFactor = rootAttribute.OriginalBlindingFactor;
            byte[] originalCommitment = rootAttribute.IssuanceCommitment;
            byte[] lastTransactionKey = rootAttribute.LastTransactionKey;
            byte[] lastBlindingFactor = rootAttribute.LastBlindingFactor;
            byte[] lastCommitment = rootAttribute.LastCommitment;
            byte[] lastDestinationKey = rootAttribute.LastDestinationKey;

            RequestInput requestInput = new RequestInput
            {
                AssetId = assetId,
                EligibilityBlindingFactor = originalBlindingFactor,
                EligibilityCommitment = originalCommitment,
                Issuer = issuer,
                PrevAssetCommitment = lastCommitment,
                PrevBlindingFactor = lastBlindingFactor,
                PrevDestinationKey = lastDestinationKey,
                PrevTransactionKey = lastTransactionKey,
                PublicSpendKey = target
            };

            RequestResult requestResult = await transactionsService.SendRevokeIdentity(requestInput, new byte[][] { rootAttribute.AnchoringOriginationCommitment }).ConfigureAwait(false);
        }

        /*[HttpPost("SendDocumentSignRequest")]
        public async Task<IActionResult> SendDocumentSignRequest(long accountId, [FromBody] UserAttributeTransferDto userAttributeTransfer)
        {
            Persistency persistency = _executionContextManager.ResolveExecutionServices(accountId);

            var transactionsService = persistency.Scope.ServiceProvider.GetService<IStealthTransactionsService>();
            (bool proceed, BiometricProof biometricProof) = await CheckBiometrics(userAttributeTransfer, accountId).ConfigureAwait(false);

            if (proceed)
            {
                await SendDocumentSignRequest(accountId, userAttributeTransfer, transactionsService, biometricProof).ConfigureAwait(false);

                return Ok(true);
            }

            return Ok(false);
        }*/

        private async Task<(BiometricPersonDataForSignatureDto dataForSignature, byte[] sourceImageBlindingFactor)> GetInputDataForBiometricSignature(UserAttributeTransferDto userAttributeTransfer, long accountId, IAssetsService assetsService)
        {
            var imageAttr = _dataAccessService.GetUserAssociatedAttributes(accountId).FirstOrDefault(t => t.Source == userAttributeTransfer.Source && t.AttributeSchemeName == AttributesSchemes.ATTR_SCHEME_NAME_PASSPORTPHOTO);

            byte[] sourceImageBytes = Convert.FromBase64String(imageAttr.Content);
            byte[] sourceImageAssetId = await assetsService.GenerateAssetId(AttributesSchemes.ATTR_SCHEME_NAME_PASSPORTPHOTO, imageAttr.Content, userAttributeTransfer.Source).ConfigureAwait(false);
            byte[] sourceImageBlindingFactor = CryptoHelper.GetRandomSeed();
            byte[] sourceImageCommitment = CryptoHelper.GetAssetCommitment(sourceImageBlindingFactor, sourceImageAssetId);
            SurjectionProof surjectionProof = CryptoHelper.CreateNewIssuanceSurjectionProof(sourceImageCommitment, new byte[][] { sourceImageAssetId }, 0, sourceImageBlindingFactor);

            BiometricPersonDataForSignatureDto biometricPersonDataForSignature = new BiometricPersonDataForSignatureDto
            {
                ImageSource = imageAttr.Content,
                ImageTarget = userAttributeTransfer.ImageContent,
                SourceImageCommitment = sourceImageCommitment.ToHexString(),
                SourceImageProofCommitment = surjectionProof.AssetCommitments[0].ToHexString(),
                SourceImageProofSignatureE = surjectionProof.Rs.E.ToHexString(),
                SourceImageProofSignatureS = surjectionProof.Rs.S[0].ToHexString()
            };

            return (biometricPersonDataForSignature, sourceImageBlindingFactor);
        }

        /*[HttpPost("Relation")]
        public async Task<IActionResult> SendRelationCreationRequest(long accountId, [FromBody] RelationsCreationRequestDTO relationsCreation)
        {
            UserRootAttribute userRootAttribute = _dataAccessService.GetUserRootAttribute(relationsCreation.UserAttributeId);
            string assetId = userRootAttribute.AssetId.ToHexString();
            var persistency = _executionContextManager.ResolveExecutionServices(accountId);
            var transactionsService = persistency.Scope.ServiceProvider.GetService<IStealthTransactionsService>();

            (bool proceed, BiometricProof biometricProof) = await CheckBiometrics(relationsCreation, accountId).ConfigureAwait(false);

            if (proceed)
            {
                var boundedAssetsService = persistency.Scope.ServiceProvider.GetService<IBoundedAssetsService>();

                var bf = _stealthClientCryptoService.GetBlindingFactor(userRootAttribute.LastTransactionKey);
                var rootIssuer = await boundedAssetsService.GetAttributeProofs(bf, userRootAttribute, _identityKeyProvider.GetKey(userRootAttribute.Source.HexStringToByteArray())).ConfigureAwait(false);

                var universalProofs = new UniversalProofs
                {
                    SessionKey = string.Empty,
                    Mission = UniversalProofsMission.RelationCreation,
                    MainIssuer = rootIssuer.Issuer,
                    RootIssuers = new List<RootIssuer> { rootIssuer },
                    Payload = payload
                };

                await SendRelationsCreationRequest(accountId, relationsCreation, transactionsService, biometricProof).ConfigureAwait(false);

                // TODO: it will be create a DID Resolver Service that will receive DID and will return a DID document with all required information
                string[] categoryEntries = relationsCreation.ExtraInfo.Split("/");

                foreach (string categoryEntry in categoryEntries)
                {
                    string groupOwnerName = categoryEntry.Split("|")[0];
                    string groupName = categoryEntry.Split("|")[1];

                    long groupRelationId = _dataAccessService.AddUserGroupRelation(accountId, groupOwnerName, relationsCreation.Target, groupName, assetId, relationsCreation.Source);

                    if (groupRelationId > 0)
                    {
                        GroupRelationDto groupRelationDto = new GroupRelationDto
                        {
                            GroupRelationId = groupRelationId,
                            GroupOwnerName = groupOwnerName,
                            GroupOwnerKey = relationsCreation.Target,
                            GroupName = groupName,
                            Issuer = relationsCreation.Source,
                            AssetId = assetId
                        };

                        await _idenitiesHubContext.Clients.Group(accountId.ToString(CultureInfo.InvariantCulture)).SendAsync("PushGroupRelation", groupRelationDto).ConfigureAwait(false);

                        await _schemeResolverService.StoreGroupRelation(relationsCreation.Source, assetId, relationsCreation.Target, groupName).ConfigureAwait(false);
                    }
                }


                return Ok(true);
            }

            return Ok(false);
        }*/


        private async Task<(bool proceed, BiometricProof biometricProof)> CheckBiometrics(UserAttributeTransferDto userAttributeTransfer, long accountId)
        {
            var scope = _executionContextManager.ResolveExecutionServices(accountId).Scope;
            var assetsService = scope.ServiceProvider.GetService<IAssetsService>();

            bool proceed = true;
            BiometricProof biometricProof = null;

            if (!string.IsNullOrEmpty(userAttributeTransfer.ImageContent) && !string.IsNullOrEmpty(userAttributeTransfer.Content))
            {
                (BiometricPersonDataForSignatureDto biometricPersonDataForSignature, byte[] sourceImageBlindingFactor) = await GetInputDataForBiometricSignature(userAttributeTransfer, accountId, assetsService).ConfigureAwait(false);

                try
                {
                    //BiometricSignedVerificationDto biometricSignedVerification = _restApiConfiguration.BiometricUri.AppendPathSegment("SignPersonFaceVerification").PostJsonAsync(biometricPersonDataForSignature).ReceiveJson<BiometricSignedVerificationDto>().Result;

                    //biometricProof = await GetBiometricProof(biometricPersonDataForSignature, biometricSignedVerification, userAttributeTransfer.Content, userAttributeTransfer.Password, sourceImageBlindingFactor).ConfigureAwait(false);
                }
                catch (FlurlHttpException)
                {
                    proceed = false;
                }

                proceed = true;
            }

            return (proceed, biometricProof);
        }

        private async Task<BiometricProof> GetBiometricProof(BiometricPersonDataForSignatureDto biometricPersonDataForSignature, BiometricSignedVerificationDto biometricSignedVerification, string rootAttributeContent, string password, byte[] sourceImageBlindingFactor, IAssetsService assetsService)
        {
            byte[] assetId = await assetsService.GenerateAssetId(AttributesSchemes.ATTR_SCHEME_NAME_PASSPORTPHOTO, biometricPersonDataForSignature.ImageSource, null).ConfigureAwait(false);
            assetsService.GetBlindingPoint(CryptoHelper.PasswordHash(password), assetId, out byte[] blindingPoint, out byte[] blindingFactor);

            byte[] photoIssuanceCommitment = assetsService.GetCommitmentBlindedByPoint(assetId, blindingPoint);
            byte[] sourceImageCommitment = biometricPersonDataForSignature.SourceImageCommitment.HexStringToByteArray();
            byte[] diffBF = CryptoHelper.GetDifferentialBlindingFactor(sourceImageBlindingFactor, blindingFactor);
            SurjectionProof surjectionProof = CryptoHelper.CreateSurjectionProof(sourceImageCommitment, new byte[][] { photoIssuanceCommitment }, 0, diffBF);

            return new BiometricProof
            {
                BiometricCommitment = sourceImageCommitment,
                BiometricSurjectionProof = surjectionProof,
                VerifierPublicKey = biometricSignedVerification.PublicKey.HexStringToByteArray(),
                VerifierSignature = biometricSignedVerification.Signature.HexStringToByteArray()
            };
        }

        [HttpPost("UniversalProofs")]
        public async Task<IActionResult> SendUniversalProofs([FromQuery] long accountId, [FromBody] UniversalProofsSendingRequest request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if(request.IdentityPools is null)
            {
                throw new ArgumentException($"{nameof(request.IdentityPools)} cannot be empty", nameof(request.IdentityPools));
            }

            if(request.Mission == null)
            {
                return BadRequest("Mission is missing");
            }

            UniversalProofs universalProofs = new UniversalProofs
            {
                SessionKey = request.SessionKey,
                Mission = (UniversalProofsMission)request.Mission
            };
            
            RequestInput requestInput = null;
            foreach (var pool in request.IdentityPools)
            {
                var rootIssuer = await GenerateFromIdentityPool(accountId, pool, request.Target).ConfigureAwait(false);
                universalProofs.RootIssuers.Add(rootIssuer);

                if(request.RootAttributeId == pool.RootAttributeId)
                {
                    var rootAttribute = _dataAccessService.GetUserRootAttribute(pool.RootAttributeId);
                    requestInput = new RequestInput
                    {
                        AssetId = rootAttribute.AssetId,
                        Issuer = rootAttribute.Source.HexStringToByteArray(),
                        PrevAssetCommitment = rootAttribute.LastCommitment,
                        PrevBlindingFactor = rootAttribute.LastBlindingFactor,
                        PrevDestinationKey = rootAttribute.LastDestinationKey,
                        PrevTransactionKey = rootAttribute.LastTransactionKey,
                        PublicSpendKey = request.Target.ToByteArray(),
                        AssetCommitment = rootIssuer.IssuersAttributes.Find(i => i.Issuer.Equals(rootIssuer.Issuer)).RootAttribute.Commitment,
                    };

                    universalProofs.MainIssuer = rootIssuer.Issuer;
                }
            }

            var persistency = _executionContextManager.ResolveExecutionServices(accountId);
            await SendUniversalTransport(accountId, persistency.Scope.ServiceProvider, requestInput, universalProofs, request.ServiceProviderInfo, true).ConfigureAwait(false);

            return Ok();
        }

        private async Task<RootIssuer> GenerateFromIdentityPool(long accountId, UniversalProofsSendingRequest.IdentityPool identityPool, IKey target)
        {
            var persistency = _executionContextManager.ResolveExecutionServices(accountId);

            var boundedAssetsService = persistency.Scope.ServiceProvider.GetService<IBoundedAssetsService>();
            var rootAttribute = _dataAccessService.GetUserRootAttribute(identityPool.RootAttributeId);
            var associatedAttributes = _dataAccessService.GetUserAssociatedAttributes(accountId).Where(a => identityPool.AssociatedAttributes?.Contains(a.UserAssociatedAttributeId) ?? false);

            var rootIssuer = await boundedAssetsService.GetAttributeProofs(_stealthClientCryptoService.GetBlindingFactor(rootAttribute.LastTransactionKey),
                                                                           rootAttribute,
                                                                           target,
                                                                           associatedAttributes,
                                                                           true).ConfigureAwait(false);
            
            return rootIssuer;
        }

        private async Task SendUniversalTransport(long accountId, IServiceProvider serviceProvider, RequestInput requestInput, UniversalProofs universalProofs, string serviceProviderInfo, bool storeRegistration = false)
        {
            var transactionsService = serviceProvider.GetService<IStealthTransactionsService>();
            await transactionsService.SendUniversalTransaction(requestInput, universalProofs).ConfigureAwait(false);

            string universalProofsStringify = JsonConvert.SerializeObject(universalProofs);

            bool postSucceeded = false;
            await _restApiConfiguration
                .UniversalProofsPoolUri.PostJsonAsync(universalProofs)
                .ContinueWith(t =>
                {
                    if (!t.IsCompletedSuccessfully)
                    {
                        string response = AsyncUtil.RunSync(async () => await ((FlurlHttpException)t.Exception.InnerException).Call.Response.Content.ReadAsStringAsync().ConfigureAwait(false));
                        _logger.Error($"Failure during posting Universal Proofs", t.Exception.InnerException);
                        throw new UniversalProofsSendingFailedException(t.Exception.InnerException.Message, t.Exception.InnerException);

                    }
                    else
                    {
                        postSucceeded = true;
                    }
                }, TaskScheduler.Current)
                .ConfigureAwait(false);

            if (postSucceeded && storeRegistration)
            {
                await StoreRegistration(accountId, serviceProvider, requestInput.PublicSpendKey, serviceProviderInfo, requestInput.Issuer, requestInput.AssetId).ConfigureAwait(false);
            }
        }

        private async Task<bool> StoreRegistration(long accountId, IServiceProvider serviceProvider, byte[] target, string spInfo, Memory<byte> issuer, Memory<byte> assetId)
        {
            string issuerStr = issuer.ToHexString();
            string assetIdStr = assetId.ToHexString();

            _logger.LogIfDebug(() => $"Storing user registration at {spInfo}, assetId: {assetIdStr}, issuer: {issuerStr}");

            var boundedAssetsService = serviceProvider.GetService<IBoundedAssetsService>();
            (_, byte[] registrationCommitment) = await boundedAssetsService.GetBoundedCommitment(target, assetId).ConfigureAwait(false);
            long registrationId = _dataAccessService.AddUserRegistration(accountId, registrationCommitment.ToHexString(), spInfo, assetIdStr, issuerStr);
            if (registrationId > 0)
            {
                _logger.LogIfDebug(() => $"New user registration {registrationCommitment.ToHexString()} added for {spInfo}, assetId: {assetIdStr}, issuer: {issuerStr}");
                try
                {
                    bool res = await _schemeResolverService.StoreRegistrationCommitment(issuerStr, assetIdStr, registrationCommitment.ToHexString(), spInfo).ConfigureAwait(false);
                    if (!res)
                    {
                        _logger.Error($"Failed to store user registration remotely, registration: {registrationCommitment.ToHexString()}, spInfo: {spInfo}, assetId: {assetIdStr}, issuer: {issuerStr}");
                        _dataAccessService.RemoveUserRegistration(registrationId);
                    }
                    else
                    {
                        _logger.LogIfDebug(() => $"New user registration at {spInfo} stored successfully");
                    }

                    return res;
                }
                catch (Exception ex)
                {
                    _logger.Error("Failed to store Inherence Registration Commitment", ex);
                }
            }
            else
            {
                _logger.LogIfDebug(() => $"User registration {registrationCommitment.ToHexString()} at {spInfo} already exists");
            }

            return true;
        }

        /*private async Task SendDocumentSignRequest(long accountId, UserAttributeTransferDto userAttributeTransfer, IStealthTransactionsService transactionsService, BiometricProof biometricProof, AssociatedProofPreparation[] associatedProofPreparations = null)
        {
            var scope = _executionContextManager.ResolveExecutionServices(accountId).Scope;
            var assetsService = scope.ServiceProvider.GetService<IAssetsService>();
            (byte[] issuer, DocumentSignRequestInput requestInput) = GetRequestInput<DocumentSignRequestInput>(userAttributeTransfer, biometricProof);
            string[] extraInfo = userAttributeTransfer.ExtraInfo.Split('|');
            byte[] groupIssuer = extraInfo[0].HexStringToByteArray();
            byte[] groupAssetId = await assetsService.GenerateAssetId(AttributesSchemes.ATTR_SCHEME_NAME_RELATIONGROUP, extraInfo[0] + extraInfo[1], userAttributeTransfer.Target).ConfigureAwait(false);
            byte[] documentHash = extraInfo[2].HexStringToByteArray();
            ulong documentRecordHeight = ulong.Parse(extraInfo[3]);
            requestInput.GroupIssuer = groupIssuer;
            requestInput.GroupAssetId = groupAssetId;
            requestInput.DocumentHash = documentHash;
            requestInput.DocumentRecordHeight = documentRecordHeight;


            OutputSources[] outputModels = await _gatewayService.GetOutputs(_restApiConfiguration.RingSize + 1).ConfigureAwait(false);
            byte[][] issuanceCommitments = await _gatewayService.GetIssuanceCommitments(issuer, _restApiConfiguration.RingSize + 1).ConfigureAwait(false);
            RequestResult requestResult = await transactionsService.SendDocumentSignRequest(requestInput, associatedProofPreparations, outputModels, issuanceCommitments).ConfigureAwait(false);
        }*/

        /*private async Task SendRelationsCreationRequest(long accountId, RelationsCreationRequestDTO relationsCreation, IStealthTransactionsService transactionsService, BiometricProof biometricProof, AssociatedProofPreparation[] associatedProofPreparations = null)
        {
            var scope = _executionContextManager.ResolveExecutionServices(accountId).Scope;
            var assetsService = scope.ServiceProvider.GetService<IAssetsService>();
            (byte[] issuer, EmployeeRequestInput requestInput) = GetRequestInput<EmployeeRequestInput>(relationsCreation, biometricProof);

            string[] categoryEntries = relationsCreation.ExtraInfo.Split("/");
            foreach (string categoryEntry in categoryEntries)
            {
                string groupName = categoryEntry.Split("|")[1];
                bool isRegistered = "true".Equals(categoryEntry.Split("|")[2], StringComparison.InvariantCultureIgnoreCase);

                //if (!isRegistered)
                {
                    // TODO: groupAssetId will be generated from a group id rather than from group name
                    byte[] groupAssetId = await assetsService.GenerateAssetId(AttributesSchemes.ATTR_SCHEME_NAME_RELATIONGROUP, relationsCreation.Target + groupName, relationsCreation.Target).ConfigureAwait(false);
                    requestInput.GroupAssetId = groupAssetId;

                    OutputSources[] outputModels = await _gatewayService.GetOutputs(_restApiConfiguration.RingSize + 1).ConfigureAwait(false);
                    byte[][] issuanceCommitments = await _gatewayService.GetIssuanceCommitments(issuer, _restApiConfiguration.RingSize + 1).ConfigureAwait(false);

                    // TODO: need to use Universal Proofs
                    RequestResult requestResult = await transactionsService.SendUniversalTransaction(requestInput, associatedProofPreparations, outputModels, issuanceCommitments).ConfigureAwait(false);
                }
            }
        }*/

        [HttpGet("UserAssociatedAttributes")]
        public async Task<IActionResult> GetUserAssociatedAttributes(long accountId, string issuer)
        {
            var scope = _executionContextManager.ResolveExecutionServices(accountId).Scope;
            var assetsService = scope.ServiceProvider.GetService<IAssetsService>();
            var associatedAttributeSchemes = await assetsService.GetAssociatedAttributeDefinitions(issuer).ConfigureAwait(false);
            var associatedAttributes = _dataAccessService.GetUserAssociatedAttributes(accountId).ToList();

            return Ok(associatedAttributeSchemes
                .Where(a => a.SchemeName != AttributesSchemes.ATTR_SCHEME_NAME_PASSWORD)
                .Select(
                    a => new UserAssociatedAttributeDto
                    {
                        SchemeName = a.SchemeName,
                        Alias = a.Alias,
                        Content = associatedAttributes.Find(attr => attr.Source == issuer && attr.AttributeSchemeName == a.SchemeName)?.Content ?? string.Empty,
                        AttributeId = associatedAttributes.Find(attr => attr.Source == issuer && attr.AttributeSchemeName == a.SchemeName)?.UserAssociatedAttributeId ?? 0
                    }));
        }

        private static string ResolveValue(IEnumerable<(string key1, string value1)> items, string key2, string value2 = null)
        {
            foreach (var (key1, value1) in items)
            {
                if (key1 == key2)
                {
                    return value1;
                }
            }
            return value2 ?? key2;
        }

        [HttpPost("UserAssociatedAttributes")]
        public IActionResult UpdateUserAssociatedAttributes(long accountId, string issuer, [FromBody] UserAssociatedAttributeDto[] userAssociatedAttributeDtos)
        {
            _dataAccessService.UpdateUserAssociatedAttributes(accountId, issuer, userAssociatedAttributeDtos.Select(a => new Tuple<string, string>(a.SchemeName, a.Content)));

            return Ok();
        }

        [HttpPost("UserRootAttribute")]
        public IActionResult SetUserRootAttribute(long accountId, [FromBody] UserAttributeDto userAttribute)
        {
            return Ok(_dataAccessService.UpdateUserAttributeContent(userAttribute.UserAttributeId, userAttribute.Content));
        }

        private (byte[] issuer, T requestInput) GetRequestInput<T>(UserAttributeTransferDto userAttributeTransfer, BiometricProof biometricProof) where T : RequestInput, new()
        {
            UserRootAttribute userRootAttribute = _dataAccessService.GetUserRootAttribute(userAttributeTransfer.UserAttributeId);
            byte[] target = userAttributeTransfer.Target.HexStringToByteArray();
            byte[] target2 = userAttributeTransfer.Target2?.HexStringToByteArray();
            byte[] payload = userAttributeTransfer.Payload?.HexStringToByteArray();
            byte[] issuer = userRootAttribute.Source.HexStringToByteArray();
            byte[] assetId = userRootAttribute.AssetId;
            byte[] originalBlindingFactor = userRootAttribute.OriginalBlindingFactor;
            byte[] originalCommitment = userRootAttribute.IssuanceCommitment;
            byte[] lastTransactionKey = userRootAttribute.LastTransactionKey;
            byte[] lastBlindingFactor = userRootAttribute.LastBlindingFactor;
            byte[] lastCommitment = userRootAttribute.LastCommitment;
            byte[] lastDestinationKey = userRootAttribute.LastDestinationKey;


            T requestInput = new T
            {
                AssetId = assetId,
                EligibilityBlindingFactor = originalBlindingFactor,
                EligibilityCommitment = originalCommitment,
                Issuer = issuer,
                PrevAssetCommitment = lastCommitment,
                PrevBlindingFactor = lastBlindingFactor,
                PrevDestinationKey = lastDestinationKey,
                PrevTransactionKey = lastTransactionKey,
                PublicSpendKey = target,
                PublicViewKey = target2,
                Payload = payload,
                BiometricProof = biometricProof
            };

            return (issuer, requestInput);
        }

        [HttpGet("UserDetails")]
        public IActionResult GetUserDetails(long accountId)
        {
            AccountDescriptor account = _accountsService.GetById(accountId);

            if (account != null)
            {
                return Ok(new
                {
                    Id = accountId.ToString(CultureInfo.InvariantCulture),
                    account.AccountInfo,
                    PublicSpendKey = account.PublicSpendKey.ToHexString(),
                    PublicViewKey = account.PublicViewKey.ToHexString(),
                    account.IsCompromised,
                    IsAutoTheftProtection = _dataAccessService.GetUserSettings(account.AccountId)?.IsAutoTheftProtection ?? false,
                    ConsentManagementHub = _restApiConfiguration.ConsentManagementUri.AppendPathSegment("consentHub").ToString()
                });
            }

            return BadRequest();
        }

        [HttpDelete("DeleteNonConfirmedRootAttribute")]
        public IActionResult DeleteNonConfirmedRootAttribute(long accountId, [FromQuery] string content)
        {
            string c = HttpUtility.HtmlDecode(content);
            if (_dataAccessService.DeleteNonConfirmedUserRootAttribute(accountId, c))
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost("IdentityRegistration")]
        public async Task<IActionResult> IdentityRegistration(long accountId, [FromBody] RequestForIdentityDto requestForIdentity)
        {
            var scope = _executionContextManager.ResolveExecutionServices(accountId).Scope;
            var assetsService = scope.ServiceProvider.GetService<IAssetsService>();
            IssuerActionDetails registrationDetails = null;

            await requestForIdentity.Target.DecodeFromString64().GetJsonAsync<IssuerActionDetails>().ContinueWith(t =>
            {
                if (t.IsCompleted && !t.IsFaulted)
                {
                    registrationDetails = t.Result;
                }
            }, TaskScheduler.Current).ConfigureAwait(false);

            if (registrationDetails == null)
            {
                return BadRequest();
            }

            AccountDescriptor account = _accountsService.GetById(accountId);
            string email = Uri.UnescapeDataString(requestForIdentity.IdCardContent);
            byte[] assetId = await assetsService.GenerateAssetId(AttributesSchemes.ATTR_SCHEME_NAME_EMAIL, email, registrationDetails.Issuer).ConfigureAwait(false);
            byte[] sessionBlindingFactor = CryptoHelper.ReduceScalar32(CryptoHelper.FastHash256(Encoding.ASCII.GetBytes(requestForIdentity.Passphrase)));
            byte[] sessionCommitment = CryptoHelper.BlindAssetCommitment(CryptoHelper.GetNonblindedAssetCommitment(assetId), sessionBlindingFactor);

            assetsService.GetBlindingPoint(CryptoHelper.PasswordHash(requestForIdentity.Password), assetId, out byte[] blindingPoint, out byte[] blindingFactor);

            string error = null;
            await registrationDetails.ActionUri.DecodeFromString64().PostJsonAsync(
                new
                {
                    Content = requestForIdentity.IdCardContent,
                    BlindingPoint = blindingPoint.ToHexString(),
                    SessionCommitment = sessionCommitment.ToHexString(),
                    requestForIdentity.ImageContent,
                    PublicSpendKey = account.PublicSpendKey.ToHexString(),
                    PublicViewKey = account.PublicViewKey.ToHexString()
                }).ContinueWith(t =>
                {
                    if (t.IsCompleted && !t.IsFaulted && t.Result.IsSuccessStatusCode)
                    {
                        _dataAccessService.AddNonConfirmedRootAttribute(accountId, requestForIdentity.IdCardContent, registrationDetails.Issuer, AttributesSchemes.ATTR_SCHEME_NAME_EMAIL, assetId);

                        _dataAccessService.UpdateUserAssociatedAttributes(accountId, registrationDetails.Issuer, new List<Tuple<string, string>> { new Tuple<string, string>(AttributesSchemes.ATTR_SCHEME_NAME_PASSPORTPHOTO, requestForIdentity.ImageContent) });
                    }
                    else
                    {
                        error = t.Result.Content.ReadAsStringAsync().Result;
                    }
                }, TaskScheduler.Current).ConfigureAwait(false);

            if (string.IsNullOrEmpty(error))
            {
                return Ok();
            }

            return BadRequest(error);
        }

        [HttpPost("{accountId}/Attributes")]
        public async Task<IActionResult> RequestForAttributesIssuance(long accountId, [FromBody] AttributesIssuanceRequestDto attributesIssuanceRequest)
        {
            _logger.Info("RequestForAttributesIssuance started");
            var scope = _executionContextManager.ResolveExecutionServices(accountId).Scope;
            var assetsService = scope.ServiceProvider.GetService<IAssetsService>();

            try
            {
                var account = _accountsService.GetById(accountId);
                var persistency = _executionContextManager.ResolveExecutionServices(accountId);
                var boundedAssetsService = persistency.Scope.ServiceProvider.GetService<IBoundedAssetsService>();
                
                var attributes = attributesIssuanceRequest.AttributeValues;
                var issuer = attributesIssuanceRequest.Issuer;

                var rootAttributeDefinition = await assetsService.GetRootAttributeDefinition(attributesIssuanceRequest.Issuer).ConfigureAwait(false);
                if (rootAttributeDefinition == null)
                {
                    throw new NoRootAttributeSchemeDefinedException(attributesIssuanceRequest.Issuer);
                }
                else
                {
                    _logger.Debug("rootAttributeDefinition obtained");
                }

                byte[] blindingPointRootToRoot = null;
                byte[] masterRootAttributeId = null;

                if (attributesIssuanceRequest.MasterRootAttributeId != null)
                {
                    _logger.Debug("attributesIssuanceRequest.MasterRootAttributeId != null");
                    var rootAttributeMaster = _dataAccessService.GetUserRootAttribute(attributesIssuanceRequest.MasterRootAttributeId.Value);
                    byte[] blindingPointRoot = assetsService.GetBlindingPoint(await boundedAssetsService.GetBindingKey().ConfigureAwait(false), rootAttributeMaster.AssetId);
                    blindingPointRootToRoot = assetsService.GetCommitmentBlindedByPoint(rootAttributeMaster.AssetId, blindingPointRoot);
                    masterRootAttributeId = rootAttributeMaster.AssetId;
                }
                else
                {
                    _logger.Debug("attributesIssuanceRequest.MasterRootAttributeId == null");
                }

                string rootAttributeContent = attributes.FirstOrDefault(a => a.Key == rootAttributeDefinition.AttributeName).Value;
                if (string.IsNullOrEmpty(rootAttributeContent))
                {
                    throw new NoValueForAttributeException(rootAttributeDefinition.AttributeName);
                }

                byte[] rootAssetId = assetsService.GenerateAssetId(rootAttributeDefinition.SchemeId, rootAttributeContent);
                _logger.Debug($"rootAssetId = {rootAssetId?.ToHexString()??"NULL"}");

                IssueAttributesRequestDTO request = new IssueAttributesRequestDTO
                {
                    Attributes = await GenerateAttributeValuesAsync(assetsService, attributes, rootAssetId, rootAttributeDefinition.AttributeName, issuer, blindingPointRootToRoot).ConfigureAwait(false),
                    PublicSpendKey = attributesIssuanceRequest.MasterRootAttributeId == null ? account.PublicSpendKey.ToHexString() : null,
                    PublicViewKey = attributesIssuanceRequest.MasterRootAttributeId == null ? account.PublicViewKey.ToHexString() : null,
                };

                if (attributesIssuanceRequest.MasterRootAttributeId == null)
                {
                    // Need only in case when _rootAttribute is null
                    // =======================================================================================================================
                    byte[] protectionAssetId = await assetsService.GenerateAssetId(AttributesSchemes.ATTR_SCHEME_NAME_PASSWORD, rootAssetId.ToHexString(), issuer).ConfigureAwait(false);
                    assetsService.GetBlindingPoint(await boundedAssetsService.GetBindingKey().ConfigureAwait(false), rootAssetId, protectionAssetId, out byte[] blindingPoint, out byte[] blindingFactor);
                    byte[] protectionAssetNonBlindedCommitment = CryptoHelper.GetNonblindedAssetCommitment(protectionAssetId);
                    byte[] protectionAssetCommitment = CryptoHelper.SumCommitments(protectionAssetNonBlindedCommitment, blindingPoint);
                    byte[] sessionBlindingFactor = CryptoHelper.GetRandomSeed();
                    byte[] sessionCommitment = CryptoHelper.GetAssetCommitment(sessionBlindingFactor, protectionAssetId);
                    byte[] diffBlindingFactor = CryptoHelper.GetDifferentialBlindingFactor(sessionBlindingFactor, blindingFactor);
                    SurjectionProof surjectionProof = CryptoHelper.CreateSurjectionProof(sessionCommitment, new byte[][] { protectionAssetCommitment }, 0, diffBlindingFactor);
                    // =======================================================================================================================

                    byte[] bindingKey = await boundedAssetsService.GetBindingKey().ConfigureAwait(false);
                    byte[] blindingPointAssociatedToParent = assetsService.GetBlindingPoint(bindingKey, rootAssetId);
                    request.Attributes.Add(AttributesSchemes.ATTR_SCHEME_NAME_PASSWORD, new IssueAttributesRequestDTO.AttributeValue
                    {
                        BlindingPointValue = blindingPoint,
                        BlindingPointRoot = blindingPointAssociatedToParent,
                        Value = rootAssetId.ToHexString()
                    });
                    request.Protection = new IssuanceProtection
                    {
                        SessionCommitment = sessionCommitment.ToHexString(),
                        SignatureE = surjectionProof.Rs.E.ToHexString(),
                        SignatureS = surjectionProof.Rs.S[0].ToHexString()
                    };
                }

                var attributeValues =
                    await _portalConfiguration
                    .IdentityProviderUri
                    .AppendPathSegments("IssueIdpAttributes", issuer)
                    .PostJsonAsync(request)
                    .ReceiveJson<IEnumerable<AttributeValue>>()
                    .ConfigureAwait(false);


                if (attributesIssuanceRequest.MasterRootAttributeId == null)
                {
                    var rootAttributeValue = attributeValues.FirstOrDefault(v => v.Definition.IsRoot);
                    if (rootAttributeValue != null)
                    {
                        _dataAccessService.AddNonConfirmedRootAttribute(accountId, rootAttributeValue.Value, issuer, rootAttributeValue.Definition.AttributeName, rootAssetId);
                    }
                }

                _dataAccessService.UpdateUserAssociatedAttributes(accountId,
                                                                  issuer,
                                                                  attributeValues.Where(a => attributesIssuanceRequest.MasterRootAttributeId != null || !a.Definition.IsRoot).Select(a => new Tuple<string, string>(a.Definition.AttributeName, a.Value)),
                                                                  masterRootAttributeId ?? rootAssetId);

                return Ok(attributeValues);

                async Task<Dictionary<string, IssueAttributesRequestDTO.AttributeValue>> GenerateAttributeValuesAsync(IAssetsService assetsService, Dictionary<string, string> attributes, byte[] rootAssetId, string rootAttributeName, string issuer, byte[] blindingPointRootToRoot)
                {
                    byte[] bindingKey = await boundedAssetsService.GetBindingKey().ConfigureAwait(false);
                    byte[] blindingPointAssociatedToParent = assetsService.GetBlindingPoint(bindingKey, rootAssetId);
                    var associateAttributeDefinitions = await assetsService.GetAssociatedAttributeDefinitions(issuer).ConfigureAwait(false);
                    var rootAttributeDefinition = await assetsService.GetRootAttributeDefinition(issuer).ConfigureAwait(false);
                    return attributes
                            .Select(kv =>
                                new KeyValuePair<string, IssueAttributesRequestDTO.AttributeValue>(
                                    kv.Key,
                                    new IssueAttributesRequestDTO.AttributeValue
                                    {
                                        Value = kv.Value,
                                        BlindingPointValue =
                                            assetsService.GetBlindingPoint(bindingKey, rootAssetId,
                                                assetsService.GenerateAssetId(rootAttributeDefinition.AttributeName == kv.Key ? rootAttributeDefinition.SchemeId : associateAttributeDefinitions.FirstOrDefault(a => a.AttributeName == kv.Key).SchemeId, kv.Value)),
                                        BlindingPointRoot = kv.Key == rootAttributeName ? blindingPointRootToRoot : blindingPointAssociatedToParent
                                    }))
                            .ToDictionary(kv => kv.Key, kv => kv.Value);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("RequestForAttributesIssuance failed", ex);
                throw;
            }
            finally
            {
                _logger.Info("RequestForAttributesIssuance ended");
            }
        }

        [HttpPost("RequestForIdentity")]
        public async Task<IActionResult> RequestForIdentity(long accountId, [FromBody] RequestForIdentityDto requestForIdentity)
        {
            var scope = _executionContextManager.ResolveExecutionServices(accountId).Scope;
            var assetsService = scope.ServiceProvider.GetService<IAssetsService>();

            try
            {
                string actionDetailsUri = requestForIdentity.Target.DecodeFromString64();
                IssuerActionDetails actionDetails = await GetActionDetails(actionDetailsUri).ConfigureAwait(false);

                if (actionDetails == null)
                {
                    _logger.Error($"[{accountId}]: request to {actionDetailsUri} failed");
                    return BadRequest();
                }

                AccountDescriptor account = _accountsService.GetById(accountId);

                var rootAttributeDefinition = await assetsService.GetRootAttributeDefinition(actionDetails.Issuer).ConfigureAwait(false);
                byte[] rootAssetId = assetsService.GenerateAssetId(rootAttributeDefinition.SchemeId, Uri.UnescapeDataString(requestForIdentity.IdCardContent));
                byte[] protectionAssetId = await assetsService.GenerateAssetId(AttributesSchemes.ATTR_SCHEME_NAME_PASSWORD, rootAssetId.ToHexString(), actionDetails.Issuer).ConfigureAwait(false);

                assetsService.GetBlindingPoint(CryptoHelper.PasswordHash(requestForIdentity.Password), protectionAssetId, out byte[] blindingPoint, out byte[] blindingFactor);

                byte[] protectionAssetNonBlindedCommitment = CryptoHelper.GetNonblindedAssetCommitment(protectionAssetId);
                byte[] protectionAssetCommitment = CryptoHelper.SumCommitments(protectionAssetNonBlindedCommitment, blindingPoint);
                byte[] sessionBlindingFactor = CryptoHelper.GetRandomSeed();
                byte[] sessionCommitment = CryptoHelper.GetAssetCommitment(sessionBlindingFactor, protectionAssetId);
                byte[] diffBlindingFactor = CryptoHelper.GetDifferentialBlindingFactor(sessionBlindingFactor, blindingFactor);
                SurjectionProof surjectionProof = CryptoHelper.CreateSurjectionProof(sessionCommitment, new byte[][] { protectionAssetCommitment }, 0, diffBlindingFactor);

                IdentityBaseData identityRequest = new IdentityBaseData
                {
                    PublicSpendKey = account.PublicSpendKey.ToHexString(),
                    PublicViewKey = account.PublicViewKey.ToHexString(),
                    Content = requestForIdentity.IdCardContent,
                    Protection = new IssuanceProtection
                    {
                        SessionCommitment = sessionCommitment.ToHexString(),
                        SignatureE = surjectionProof.Rs.E.ToHexString(),
                        SignatureS = surjectionProof.Rs.S[0].ToHexString()
                    },
                    BlindingPoint = blindingPoint.ToHexString(),
                    ImageContent = requestForIdentity.ImageContent
                };

                string error = null;

                string uri = actionDetails.ActionUri.DecodeFromString64();
                try
                {
                    _logger.LogIfDebug(() => $"[{accountId}]: Requesting Identity with URI {uri} and session data {JsonConvert.SerializeObject(identityRequest, new ByteArrayJsonConverter())}");
                    await uri.PostJsonAsync(identityRequest).ContinueWith(t =>
                    {
                        if (t.IsCompletedSuccessfully)
                        {
                            byte[] assetId = assetsService.GenerateAssetId(rootAttributeDefinition.SchemeId, requestForIdentity.IdCardContent);
                            _dataAccessService.AddNonConfirmedRootAttribute(accountId, requestForIdentity.IdCardContent, actionDetails.Issuer, rootAttributeDefinition.SchemeName, assetId);

                            if (!string.IsNullOrEmpty(requestForIdentity.ImageContent))
                            {
                                _dataAccessService.UpdateUserAssociatedAttributes(accountId, actionDetails.Issuer, new List<Tuple<string, string>> { new Tuple<string, string>(AttributesSchemes.ATTR_SCHEME_NAME_PASSPORTPHOTO, requestForIdentity.ImageContent) });
                            }
                        }
                        else
                        {
                            error = t.ReceiveString().Result;
                            _logger.Error($"Failure during querying {actionDetails.ActionUri.DecodeFromString64()}, error: {(error ?? "NULL")}", t.Exception);
                        }
                    }, TaskScheduler.Current).ConfigureAwait(false);

                }
                catch (Exception ex)
                {
                    _logger.Error($"Failure during sending request to URI {uri} with body {JsonConvert.SerializeObject(identityRequest)}", ex);
                    throw;
                }
                if (string.IsNullOrEmpty(error))
                {
                    return Ok();
                }

                return BadRequest(error);

            }
            catch (Exception ex)
            {
                _logger.Error($"Failure in {nameof(RequestForIdentity)}", ex);
                throw;
            }
        }

        private async Task<IssuerActionDetails> GetActionDetails(string uri)
        {
            IssuerActionDetails actionDetails = null;

            //string[] authorizationValues = Request.Headers["Authorization"].ToString().Split(" ");

            await uri.GetJsonAsync<IssuerActionDetails>().ContinueWith(t =>
            {
                if (t.IsCompletedSuccessfully)
                {
                    actionDetails = t.Result;
                }
                else
                {
                    _logger.Error($"GetActionDetails, Request to {uri} failed", t.Exception);
                    foreach (var ex in t.Exception.InnerExceptions)
                    {
                        _logger.Error($"GetActionDetails, inner exception", ex);
                    }
                }
            }, TaskScheduler.Current).ConfigureAwait(false);
            return actionDetails;
        }

        [HttpGet("ActionType")]
        public IActionResult GetActionType(string actionInfo)
        {
            string actionDecoded = actionInfo.DecodeFromString64();

            if (actionDecoded.StartsWith("iss://"))
            {
                return Ok(new { Action = "12", ActionInfo = Convert.ToBase64String(Encoding.UTF8.GetBytes(actionDecoded.Replace("iss://", ""))) });
            }
            else if (actionDecoded.StartsWith("dis://"))
            {
                return Ok(new { Action = "11", ActionInfo = Convert.ToBase64String(Encoding.UTF8.GetBytes(actionDecoded.Replace("dis://", ""))) });
            }
            else if (actionDecoded.StartsWith("wreg://"))
            {
                return Ok(new { Action = "10", ActionInfo = Convert.ToBase64String(Encoding.UTF8.GetBytes(actionDecoded.Replace("wreg://", ""))) });
            }
            else if (actionDecoded.StartsWith("saml://"))
            {
                return Ok(new { Action = "2", ActionInfo = actionInfo });
            }
            else if (actionDecoded.StartsWith("prf://"))
            {
                return GetProofActionType(actionDecoded);
            }
            else if (actionDecoded.StartsWith("sig://"))
            {
                return GetSignatureValidationActionType(actionDecoded);
            }
            else if (actionDecoded.StartsWith("spp://"))
            {
                return Ok(new { Action = "2", ActionInfo = actionInfo });
            }
            else
            {
                if (actionDecoded.Contains("ProcessRootIdentityRequest", StringComparison.InvariantCultureIgnoreCase))
                {
                    return Ok(new { Action = "1", ActionInfo = actionInfo });
                }
            }

            return BadRequest();
        }

        private IActionResult GetProofActionType(string actionDecoded)
        {
            return Ok(new { Action = "8", ActionInfo = actionDecoded.Replace("prf://", "") });
        }

        private IActionResult GetSignatureValidationActionType(string actionDecoded)
        {
            return Ok(new { Action = "7", ActionInfo = actionDecoded.Replace("sig://", "") });
        }

        [HttpGet("ServiceProviderActionType")]
        public IActionResult GetServiceProviderActionType(string actionInfo)
        {
            string actionType = null;
            string actionDecoded = actionInfo.DecodeUnescapedFromString64();

            if (actionDecoded.StartsWith("spp://"))
            {
                UriBuilder uriBuilder = new UriBuilder(actionDecoded);
                actionType = HttpUtility.ParseQueryString(uriBuilder.Query)["t"];
            }
            else if (actionDecoded.StartsWith("saml://"))
            {
                actionType = "3";
            }
            else if (actionDecoded.StartsWith("cnsn://"))
            {
                actionType = "4";
            }

            return Ok(new { ActionType = actionType });
        }

        [HttpGet("ServiceProviderActionInfo")]
        public async Task<ActionResult<ServiceProviderActionAndValidationsDto>> GetServiceProviderActionInfo(long accountId, string actionInfo, long attributeId)
        {
            ServiceProviderActionAndValidationsDto serviceProviderActionAndValidations = null;
            var persistency = _executionContextManager.ResolveExecutionServices(accountId);
            var boundedAssetsService = persistency.Scope.ServiceProvider.GetService<IBoundedAssetsService>();
            string actionDecoded = actionInfo.DecodeUnescapedFromString64();


            if (actionDecoded.StartsWith("cnsn://"))
            {
                actionDecoded = actionDecoded.Replace("cnsn://", "");
                TransactionConsentRequest consentRequest = JsonConvert.DeserializeObject<TransactionConsentRequest>(actionDecoded);

                byte[] confirm = _hashCalculation.CalculateHash(Encoding.UTF8.GetBytes(consentRequest.TransactionId));
                byte[] decline = _hashCalculation.CalculateHash(confirm);

                IEnumerable<UserRootAttribute> rootAttributes = _dataAccessService.GetUserAttributes(accountId);
                UserRootAttribute rootAttribute = rootAttributes.FirstOrDefault(a =>
                {
                    boundedAssetsService.GetBoundedCommitment(a.AssetId, consentRequest.PublicSpendKey.HexStringToByteArray(), out byte[] registrationBlindingFactor, out byte[] registrationCommitment);
                    return registrationCommitment.Equals32(consentRequest.RegistrationCommitment.HexStringToByteArray());
                });

                serviceProviderActionAndValidations = new ServiceProviderActionAndValidationsDto
                {
                    IsRegistered = false,
                    PublicKey = consentRequest.PublicSpendKey,
                    PublicKey2 = consentRequest.PublicViewKey,
                    IsBiometryRequired = consentRequest.WithBiometricProof,
                    ExtraInfo = $"{consentRequest.TransactionId}|{consentRequest.Description}",
                    Validations = new List<string>(),
                    SessionKey = $"{confirm.ToHexString()}|{decline.ToHexString()}",
                    PredefinedAttributeId = rootAttribute.UserAttributeId
                };

            }
            else if (actionDecoded.StartsWith("spp://"))
            {
                actionDecoded = actionDecoded.Replace("spp://", "");

                UriBuilder uriBuilder = new UriBuilder(actionDecoded);
                NameValueCollection queryParams = HttpUtility.ParseQueryString(uriBuilder.Query);
                string actionType = queryParams["t"];

                byte[] targetBytes = queryParams["pk"]?.HexStringToByteArray();

                var rootAttribute = _dataAccessService.GetUserRootAttribute(attributeId);
                var assetId = rootAttribute.AssetId;
                var attributeContent = rootAttribute.Content;

                if (actionType == "0") // Login and register
                {
                    boundedAssetsService.GetBoundedCommitment(assetId, targetBytes, out byte[] blindingFactor, out byte[] assetCommitment);
                    queryParams["rk"] = assetCommitment.ToHexString();
                    uriBuilder.Query = queryParams.ToString();
                }
                else if (actionType == "1") // employee registration
                {
                    queryParams["rk"] = attributeContent.EncodeToString64();
                    uriBuilder.Query = queryParams.ToString();
                }

                await uriBuilder.Uri.ToString()
                    .GetJsonAsync<ServiceProviderActionAndValidationsDto>()
                    .ContinueWith(t =>
                    {
                        if (t.IsCompleted && !t.IsFaulted)
                        {
                            serviceProviderActionAndValidations = t.Result;
                        }
                    }, TaskScheduler.Current).ConfigureAwait(false);

                if (actionType == "2") // document sign
                {
                    for (int i = 0; i < serviceProviderActionAndValidations.Validations.Count; i++)
                    {
                        string item = serviceProviderActionAndValidations.Validations[i];
                        string[] validationParts = item.Split(';');
                        (string groupOwnerName, string issuer, string relationAssetId) = _dataAccessService.GetRelationUserAttributes(accountId, validationParts[0], validationParts[1]);
                        if (!string.IsNullOrEmpty(groupOwnerName) && !string.IsNullOrEmpty(issuer) && !string.IsNullOrEmpty(relationAssetId))
                        {
                            serviceProviderActionAndValidations.Validations[i] += $"|Relation to group {validationParts[1]} of {groupOwnerName}|{issuer};{relationAssetId}";
                        }
                        else
                        {
                            serviceProviderActionAndValidations.Validations[i] = null;
                        }
                    }
                }
            }
            else if (actionDecoded.StartsWith("saml://"))
            {
                //persistency.ClientCryptoService.GetBoundedCommitment(rootAttribute.AssetId, targetBytes, out byte[] blindingFactor, out byte[] assetCommitment);
                //	registrationKey = assetCommitment.ToHexString();
                //	NameValueCollection queryParams = uriBuilder.Uri.ParseQueryString();
                //	queryParams["registrationKey"] = registrationKey;
                //	uriBuilder.Query = queryParams.ToString();

                string sessionInfo = actionDecoded.Replace("saml://", "");

                UriBuilder uriBuilder = new UriBuilder(_restApiConfiguration.SamlIdpUri);
                NameValueCollection queryParams = HttpUtility.ParseQueryString(uriBuilder.Query);
                queryParams["sessionInfo"] = sessionInfo;
                uriBuilder.Query = queryParams.ToString();

                SamlIdpSessionInfo samlIdpSessionInfo = uriBuilder.Uri.ToString().AppendPathSegments("SamlIdp", "GetSessionInfo").GetJsonAsync<SamlIdpSessionInfo>().Result;
                byte[] sessionKeyBytes = new Guid(samlIdpSessionInfo.SessionKey).ToByteArray();
                byte[] sessionKeyComplemented = sessionKeyBytes.ComplementTo32();

                string validationsExpression = string.Empty;

                //if ((samlIdpSessionInfo.IdentityAttributeValidationDefinitions?.IdentityAttributeValidationDefinitions?.Count ?? 0) > 0)
                //{
                //    IEnumerable<Tuple<AttributeType, string>> attributeDescriptions = _identityAttributesService.GetAssociatedAttributeTypes();
                //    IEnumerable<Tuple<ValidationType, string>> validationDescriptions = _identityAttributesService.GetAssociatedValidationTypes();

                //    List<string> validations = new List<string>();

                //    foreach (var idenitityValidation in samlIdpSessionInfo.IdentityAttributeValidationDefinitions.IdentityAttributeValidationDefinitions)
                //    {
                //        AttributeType attributeType = (AttributeType)uint.Parse(idenitityValidation.AttributeType);
                //        ValidationType validationType = (ValidationType)uint.Parse(idenitityValidation.ValidationType);

                //        if (attributeType != AttributeType.DateOfBirth)
                //        {
                //            validations.Add(attributeDescriptions.FirstOrDefault(d => d.Item1 == attributeType)?.Item2 ?? attributeType.ToString());
                //        }
                //        else
                //        {
                //            validations.Add(validationDescriptions.FirstOrDefault(d => d.Item1 == validationType)?.Item2 ?? validationType.ToString());
                //        }
                //    }

                //    validationsExpression = ":" + string.Join("|", validations);
                //}

                serviceProviderActionAndValidations = new ServiceProviderActionAndValidationsDto
                {
                    IsRegistered = false,
                    PublicKey = samlIdpSessionInfo.TargetPublicSpendKey,
                    PublicKey2 = samlIdpSessionInfo.TargetPublicViewKey,
                    IsBiometryRequired = false,
                    ExtraInfo = string.Empty,
                    Validations = samlIdpSessionInfo.Validations,
                    SessionKey = sessionKeyComplemented.ToHexString()
                };

                if ((samlIdpSessionInfo.Validations?.Count ?? 0) > 0)
                {
                    validationsExpression = ":" + string.Join("|", samlIdpSessionInfo.Validations);
                }

            }
            return serviceProviderActionAndValidations;
        }

        [HttpPost("ClearCompromised")]
        public IActionResult ClearCompromised(long accountId)
        {
            _dataAccessService.ClearAccountCompromised(accountId);

            return Ok();
        }

        /*[HttpGet("DocumentSignatureVerification")]
        public async Task<IActionResult> GetDocumentSignatureVerification([FromQuery] string documentCreator, [FromQuery] string documentHash, [FromQuery] ulong documentRecordHeight, [FromQuery] ulong signatureRecordBlockHeight)
        {
            DocumentSignatureVerification signatureVerification = await _documentSignatureVerifier.Verify(documentCreator.HexStringToByteArray(), documentHash.HexStringToByteArray(), documentRecordHeight, signatureRecordBlockHeight).ConfigureAwait(false);

            return Ok(signatureVerification);
        }*/

        [HttpGet("GroupRelations")]
        public IActionResult GetGroupRelations(long accountId)
        {
            return Ok(_dataAccessService.GetUserGroupRelations(accountId)?
                .Select(g =>
                new GroupRelationDto
                {
                    GroupRelationId = g.UserGroupRelationId,
                    GroupOwnerName = g.GroupOwnerName,
                    GroupOwnerKey = g.GroupOwnerKey,
                    GroupName = g.GroupName,
                    Issuer = g.Issuer,
                    AssetId = g.AssetId
                }) ?? Array.Empty<GroupRelationDto>());
        }

        [HttpGet("UserRegistrations")]
        public IActionResult GetUserRegistrations(long accountId)
        {
            return Ok(_dataAccessService.GetUserRegistrations(accountId)?
                .Select(g =>
                new UserRegistrationDto
                {
                    UserRegistrationId = g.UserRegistrationId.ToString(),
                    Commitment = g.Commitment,
                    Issuer = g.Issuer,
                    AssetId = g.AssetId
                }) ?? Array.Empty<UserRegistrationDto>());
        }

        [HttpDelete("GroupRelation/{grouprelationId}")]
        public IActionResult DeleteGroupRelation(long grouprelationId)
        {
            _dataAccessService.RemoveUserGroupRelation(grouprelationId);

            return Ok();
        }

        /*[HttpPost("RelationsProofs")]
        public async Task<IActionResult> SendRelationsProofs(long accountId, [FromBody] RelationsProofsDto relationsProofs)
        {
            _logger.LogIfDebug(() => $"[{accountId}]: {nameof(SendRelationsProofs)} with {nameof(relationsProofs)}={JsonConvert.SerializeObject(relationsProofs, new ByteArrayJsonConverter())}");
            var scope = _executionContextManager.ResolveExecutionServices(accountId).Scope;
            var assetsService = scope.ServiceProvider.GetService<IAssetsService>();

            try
            {
                UserRootAttribute userRootAttribute = _dataAccessService.GetUserRootAttribute(relationsProofs.UserAttributeId);
                string assetId = userRootAttribute.AssetId.ToHexString();
                var persistency = _executionContextManager.ResolveExecutionServices(accountId);
                var transactionsService = persistency.Scope.ServiceProvider.GetService<IStealthTransactionsService>();

                (bool proceed, BiometricProof biometricProof) = (true, null);// await CheckBiometrics(relationsProofs, accountId).ConfigureAwait(false);

                if (true)
                {
                    (byte[] issuer, RelationsProofsInput requestInput) = GetRequestInput<RelationsProofsInput>(relationsProofs, biometricProof);

                    byte[] imageHash;
                    if (!string.IsNullOrEmpty(relationsProofs.ImageContent))
                    {
                        byte[] imageContent = Convert.FromBase64String(relationsProofs.ImageContent);
                        imageHash = CryptoHelper.FastHash256(imageContent);
                    }
                    else
                    {
                        imageHash = new byte[Globals.DEFAULT_HASH_SIZE];
                    }

                    AssociatedProofPreparation[] associatedProofPreparations = null;

                    if (relationsProofs.WithKnowledgeProof)
                    {

                        assetsService.GetBlindingPoint(CryptoHelper.PasswordHash(relationsProofs.Password), userRootAttribute.AssetId, out byte[] blindingPoint, out byte[] blindingFactor);

                        byte[] rootOriginatingCommitment = assetsService.GetCommitmentBlindedByPoint(userRootAttribute.AssetId, blindingPoint);
                        byte[] protectionAssetId = await assetsService.GenerateAssetId(AttributesSchemes.ATTR_SCHEME_NAME_PASSWORD, assetId, relationsProofs.Source).ConfigureAwait(false);
                        byte[] protectionAssetNonBlindedCommitment = CryptoHelper.GetNonblindedAssetCommitment(protectionAssetId);
                        byte[] protectionAssetCommitment = CryptoHelper.SumCommitments(protectionAssetNonBlindedCommitment, blindingPoint);
                        byte[] associatedBlindingFactor = CryptoHelper.GetRandomSeed();
                        byte[] associatedCommitment = CryptoHelper.GetAssetCommitment(associatedBlindingFactor, protectionAssetId);
                        AssociatedProofPreparation associatedProofPreparation = new AssociatedProofPreparation
                        {
                            SchemeName = AttributesSchemes.ATTR_SCHEME_NAME_PASSWORD,
                            Commitment = associatedCommitment,
                            CommitmentBlindingFactor = associatedBlindingFactor,
                            OriginatingAssociatedCommitment = protectionAssetCommitment,
                            OriginatingBlindingFactor = blindingFactor,
                            OriginatingRootCommitment = rootOriginatingCommitment
                        };

                        associatedProofPreparations = new AssociatedProofPreparation[] { associatedProofPreparation };
                    }

                    string sessionKey = relationsProofs.Payload;
                    await _restApiConfiguration.ConsentManagementUri
                        .AppendPathSegments("ConsentManagement", "RelationProofsData")
                        .SetQueryParam("sessionKey", sessionKey)
                        .PostJsonAsync(new RelationProofsData
                        {
                            ImageContent = relationsProofs.ImageContent,
                            RelationEntries = relationsProofs.Relations.Select(r => new RelationEntry { RelatedAssetOwnerName = r.GroupOwnerName, RelatedAssetOwnerKey = r.GroupOwnerKey, RelatedAssetName = r.GroupName }).ToArray()
                        }).ConfigureAwait(false);


                    requestInput.Payload = sessionKey.HexStringToByteArray();
                    requestInput.ImageHash = imageHash;
                    requestInput.Relations =
                            (relationsProofs.Relations
                                .Select(r =>
                                new Relation
                                {
                                    RelatedAssetOwner = r.GroupOwnerKey.HexStringToByteArray(),
                                    RelatedAssetId = assetsService.GenerateAssetId(AttributesSchemes.ATTR_SCHEME_NAME_RELATIONGROUP, r.GroupOwnerKey + r.GroupName, relationsProofs.Source).Result
                                })).ToArray();


                    OutputSources[] outputModels = await _gatewayService.GetOutputs(_restApiConfiguration.RingSize + 1).ConfigureAwait(false);
                    byte[][] issuanceCommitments = await _gatewayService.GetIssuanceCommitments(issuer, _restApiConfiguration.RingSize + 1).ConfigureAwait(false);

                    await transactionsService.SendRelationsProofs(requestInput, associatedProofPreparations, outputModels, issuanceCommitments).ConfigureAwait(false);

                    return Ok();
                }

            }
            catch (Exception ex)
            {
                _logger.Error($"[{accountId}]: failure during {nameof(SendRelationsProofs)}", ex);
                throw;
            }
        }*/

        private async Task<List<string>> GetRequiredValidations(IAssetsService assetsService, List<Tuple<string, ValidationType>> validations, string issuer)
        {
            List<string> requiredValidations = new List<string>();
            var attributeDescriptions = await assetsService.GetAssociatedAttributeDefinitions(issuer).ConfigureAwait(false);
            IEnumerable<(string validationType, string validationDescription)> validationDescriptions = _identityAttributesService.GetAssociatedValidationTypes();

            foreach (var validation in validations)
            {
                if (AttributesSchemes.ATTR_SCHEME_NAME_DATEOFBIRTH.Equals(validation.Item1))
                {
                    requiredValidations.Add(ResolveValue(validationDescriptions, validation.Item2.ToString()));
                }
                else
                {
                    requiredValidations.Add(ResolveValue(attributeDescriptions.Select(a => (a.SchemeName, a.Alias)), validation.Item1));
                }
            }

            return requiredValidations;
        }

        [HttpGet("DiscloseSecrets")]
        public ActionResult<string> DiscloseSecrets(long accountId, string password)
        {
            AccountDescriptor account = _accountsService.GetById(accountId);

            Client.Common.Entities.AccountDescriptor accountDescriptor = _accountsService.Authenticate(accountId, password);

            if (accountDescriptor != null)
            {
                string qr = $"dis://{accountDescriptor.SecretSpendKey.ToHexString()}:{accountDescriptor.SecretViewKey.ToHexString()}:{account.LastAggregatedRegistrations}";
                return Ok(new { qr = qr.EncodeToString64() });
            }

            return BadRequest();
        }

        [HttpPost("ChallengeProofs")]
        public async Task<IActionResult> ChallengeProofs(string key, [FromBody] ProofsRequest proofsRequest)
        {
            HttpResponseMessage httpResponse = await _restApiConfiguration.ConsentManagementUri
                .AppendPathSegments("ConsentManagement", "ChallengeProofs")
                .SetQueryParam("key", key)
                .PostJsonAsync(proofsRequest).ConfigureAwait(false);

            string response = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            return Ok(response);
        }

        [HttpGet("PhotoRequired")]
        public async Task<IActionResult> GetPhotoRequired(string target)
        {
            IssuerActionDetails actionDetails = await GetActionDetails(target.DecodeFromString64()).ConfigureAwait(false);

            var attributeSchemes = await _schemeResolverService.ResolveAttributeSchemes(actionDetails.Issuer).ConfigureAwait(false);

            return Ok(new { IsPhotoRequired = attributeSchemes.Any(s => s.SchemeName == AttributesSchemes.ATTR_SCHEME_NAME_PASSPORTPHOTO) });
        }

        [HttpPost("Vote")]
        public async Task<IActionResult> CastVote(long accountId, [FromBody] VoteDto vote)
        {
            if (vote is null)
            {
                throw new ArgumentNullException(nameof(vote));
            }

            var persistency = _executionContextManager.ResolveExecutionServices(accountId);
            var boundedAssetsService = persistency.Scope.ServiceProvider.GetService<IBoundedAssetsService>();

            // ================================================================================

            var poll = await _portalConfiguration.ElectionCommitteeUri
                .AppendPathSegments("Poll", vote.PollId.ToString())
                .GetJsonAsync<Poll>()
                .ConfigureAwait(false);

            var rootAttributes = _dataAccessService.GetUserAttributes(accountId);
            var rootAttributePoll = rootAttributes.FirstOrDefault(r => r.Source == poll.Issuer);
            var rootAttribute = rootAttributes.FirstOrDefault(r => !r.IsOverriden && !string.IsNullOrEmpty(r.Content));

            byte[][] assetIds = new byte[vote.CandidateAssetIds.Length][];
            int index = Array.IndexOf(vote.CandidateAssetIds, vote.SelectedAssetId);
            byte[][] bfs = new byte[vote.CandidateAssetIds.Length][];
            CandidateCommitment[] commitments = new CandidateCommitment[vote.CandidateAssetIds.Length];
            for (int i = 0; i < vote.CandidateAssetIds.Length; i++)
            {
                assetIds[i] = vote.CandidateAssetIds[i].HexStringToByteArray();
            }
            for (int i = 0; i < vote.CandidateAssetIds.Length; i++)
            {
                bfs[i] = CryptoHelper.GetRandomSeed();
                byte[] candidateCommitment = CryptoHelper.GetAssetCommitment(bfs[i], assetIds[i]);
                commitments[i] = new CandidateCommitment
                {
                    Commitment = candidateCommitment,
                    IssuanceProof = CryptoHelper.CreateNewIssuanceSurjectionProof(candidateCommitment, assetIds, i, bfs[i])
                };
            }

            byte[] selectionBf = CryptoHelper.GetRandomSeed();
            byte[] selectionCommitment = CryptoHelper.BlindAssetCommitment(commitments[index].Commitment, selectionBf);

            SelectionCommitmentRequest commitmentRequest = new SelectionCommitmentRequest
            {
                Commitment = selectionCommitment,
                CandidateCommitments = commitments,
                CandidateCommitmentProofs = CryptoHelper.CreateSurjectionProof(selectionCommitment, commitments.Select(c => c.Commitment).ToArray(), index, selectionBf)
            };

            SignedEcCommitment ecCommitment =
                await _portalConfiguration.ElectionCommitteeUri
                .AppendPathSegments("Poll", vote.PollId.ToString(), "Commitment")
                .PostJsonAsync(commitmentRequest)
                .ReceiveJson<SignedEcCommitment>()
                .ConfigureAwait(false);

            EcSurjectionProofRequest surjectionProofRequest = new EcSurjectionProofRequest
            {
                EcCommitment = ecCommitment.EcCommitment,
                CandidateCommitments = commitments.Select(c => c.Commitment).ToArray(),
                Index = index,
                PartialBlindingFactor = selectionBf
            };

            SurjectionProof ecSurjectionProof =
                await _portalConfiguration.ElectionCommitteeUri
                .AppendPathSegments("Poll", vote.PollId.ToString(), "Proof")
                .PostJsonAsync(surjectionProofRequest)
                .ReceiveJson<SurjectionProof>()
                .ConfigureAwait(false);

            byte[] voterBf = CryptoHelper.SumScalars(selectionBf, bfs[index]);

            ElectionCommitteePayload payload = new ElectionCommitteePayload
            {
                PollId = vote.PollId,
                PartialBf = voterBf,
                EcCommitment = _identityKeyProvider.GetKey(ecCommitment.EcCommitment)
            };

            var bfBase = _stealthClientCryptoService.GetBlindingFactor(rootAttributePoll.LastTransactionKey); // TODO: bfBase prevoiusly had a different generated value from the `bf`
            var rootIssuerBase = await boundedAssetsService.GetAttributeProofs(bfBase, rootAttribute, withProtectionAttribute: true).ConfigureAwait(false);
            
            var bf = _stealthClientCryptoService.GetBlindingFactor(rootAttributePoll.LastTransactionKey);
            var rootIssuerPoll = await boundedAssetsService.GetAttributeProofs(bf, rootAttributePoll).ConfigureAwait(false);

            UniversalProofs universalProofs = new UniversalProofs
            {
                SessionKey = ecCommitment.EcCommitment.ToHexString(),
                Mission = UniversalProofsMission.Vote,
                MainIssuer = rootIssuerPoll.Issuer,
                RootIssuers = new List<RootIssuer> { rootIssuerPoll, rootIssuerBase },
                Payload = payload
            };

            RequestInput requestInput = new RequestInput
            {
                AssetId = rootAttributePoll.AssetId,
                Issuer = rootAttributePoll.Source.HexStringToByteArray(),
                PrevAssetCommitment = rootAttributePoll.LastCommitment,
                PrevBlindingFactor = rootAttributePoll.LastBlindingFactor,
                PrevDestinationKey = rootAttributePoll.LastDestinationKey,
                PrevTransactionKey = rootAttributePoll.LastTransactionKey,
                PublicSpendKey = rootIssuerPoll.Issuer.ToByteArray(),
                AssetCommitment = rootIssuerPoll.IssuersAttributes[0].RootAttribute.Commitment,
            };

            await SendUniversalTransport(accountId, persistency.Scope.ServiceProvider, requestInput, universalProofs, poll.Name).ConfigureAwait(false);

            var res = await _portalConfiguration.ElectionCommitteeUri
                .AppendPathSegments("Poll", vote.PollId.ToString(), "Vote")
                .PostJsonAsync(payload)
                .ReceiveJson<VoteCastedResult>()
                .ConfigureAwait(false);

            if (res.Result && rootAttributePoll != null)
            {
                _dataAccessService.RemoveUserAttribute(accountId, rootAttributePoll.UserAttributeId);
            }

            return Ok();
        }
    }
}