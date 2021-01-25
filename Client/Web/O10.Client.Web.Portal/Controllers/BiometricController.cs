using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using O10.Client.Common.Identities;
using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.Services;
using O10.Core.Configuration;
using O10.Client.Web.Portal.Configuration;
using O10.Core.ExtensionMethods;
using System.Linq;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using O10.Client.Web.Common.Dtos.Biometric;
using O10.Crypto.ConfidentialAssets;
using O10.Core.Cryptography;
using O10.Client.DataLayer.AttributesScheme;
using O10.Client.Web.Portal.Services.Inherence;
using O10.Client.Common.Entities;
using O10.Client.Web.Portal.Exceptions;
using O10.Core.Logging;
using System.Diagnostics.Contracts;
using O10.Client.Web.Portal.Properties;
using System.Collections.Generic;

namespace O10.Client.Web.Portal.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BiometricController : ControllerBase
    {
        private readonly IFacesService _facesService;
        private readonly IDataAccessService _dataAccessService;
        private readonly IAssetsService _assetsService;
        private readonly IPortalConfiguration _portalConfiguration;
        private readonly IInherenceService _inherenceService;
        private readonly ILogger _logger;

        public BiometricController(IFacesService facesService,
                                   IConfigurationService configurationService,
                                   IDataAccessService externalDataAccessService,
                                   IAssetsService assetsService,
                                   ILoggerService loggerService,
                                   IInherenceServicesManager inherenceServicesManager)
        {
            if (configurationService is null)
            {
                throw new ArgumentNullException(nameof(configurationService));
            }

            if (inherenceServicesManager is null)
            {
                throw new ArgumentNullException(nameof(inherenceServicesManager));
            }

            _facesService = facesService ?? throw new ArgumentNullException(nameof(facesService));
            _facesService.Initialize();
            _dataAccessService = externalDataAccessService;
            _assetsService = assetsService;
            _logger = loggerService.GetLogger(nameof(BiometricController));
            _portalConfiguration = configurationService.Get<IPortalConfiguration>();
            _inherenceService = inherenceServicesManager.GetInstance(O10InherenceService.NAME);
        }

        [AllowAnonymous]
        [HttpDelete("RegisterPerson")]
        public async Task<IActionResult> UnregisterPerson([FromQuery] string issuer, [FromQuery] string commitment)
        {
            PersonFaceData personFaceData = new PersonFaceData
            {
                PersonGroupId = _portalConfiguration.DemoMode ? _portalConfiguration.FacePersonGroupId.ToLower() : issuer.ToLower(),
                Name = commitment,
                UserData = commitment
            };

            if (_dataAccessService.RemoveBiometricPerson(commitment))
            {
                await _facesService.RemovePerson(personFaceData).ConfigureAwait(false);
            }

            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("RegisterPerson")]
        public async Task<IActionResult> RegisterPerson([FromBody] BiometricPersonDataDto biometricPersonData)
        {
            byte[] imageContent = null;

            _logger.LogIfDebug(() => $"[{_inherenceService.AccountId}]: {nameof(RegisterPerson)}, {nameof(biometricPersonData.SessionKey)}={biometricPersonData.SessionKey}, {biometricPersonData.Images?.Count ?? -1} {nameof(biometricPersonData.Images)}");
            Contract.Requires(biometricPersonData != null, nameof(biometricPersonData));
            Contract.Requires(biometricPersonData.Images != null, $"{nameof(biometricPersonData)}.{nameof(biometricPersonData.Images)}");
            Contract.Requires(biometricPersonData.Images.Count > 0, $"Count {nameof(biometricPersonData)}.{nameof(biometricPersonData.Images)} > 0");

            foreach (var image in biometricPersonData.Images)
            {
                _logger.LogIfDebug(() => $"[{_inherenceService.AccountId}]: {nameof(RegisterPerson)}, {nameof(biometricPersonData.SessionKey)}={biometricPersonData.SessionKey}, {nameof(image.Key)}={image.Key}, image length: {image.Value?.Length ?? -1}");
            }

            TaskCompletionSource<InherenceData> taskCompletionSource = _inherenceService.GetIdentityProofsAwaiter(biometricPersonData.SessionKey);

            InherenceData inherenceData;
            if (taskCompletionSource.Task.IsCompleted)
            {
                inherenceData = taskCompletionSource.Task.Result;
            }
            else
            {
                try
                {
                    inherenceData = await taskCompletionSource.Task.TimeoutAfter(30000).ConfigureAwait(false);
                }
                catch (TimeoutException ex)
                {
                    _logger.Error($"[{_inherenceService.AccountId}]: Identity Proofs not received for the SessionKey {biometricPersonData.SessionKey}", ex);
                    return BadRequest("Identity Proofs not received");
                }
                catch (Exception ex)
                {
                    _logger.Error($"[{_inherenceService.AccountId}]: Identity Proofs failed", ex);
                    return BadRequest($"Identity Proofs failed: {ex.Message}");
                }
                finally
                {
                    _inherenceService.RemoveIdentityProofsAwaiter(biometricPersonData.SessionKey);
                }
            }

            _inherenceService.RemoveIdentityProofsAwaiter(biometricPersonData.SessionKey);

            if (inherenceData == null)
            {
                _logger.Error($"[{_inherenceService.AccountId}]: Identity Proofs failed for {nameof(biometricPersonData.SessionKey)}={biometricPersonData.SessionKey}");
                return BadRequest("Identity Proofs failed");
            }

            _logger.LogIfDebug(() => $"[{_inherenceService.AccountId}]: {nameof(RegisterPerson)}, {nameof(InherenceData)} with {nameof(biometricPersonData.SessionKey)}={biometricPersonData.SessionKey} obtained");

            string registrationKey = inherenceData.RootRegistrationProof.AssetCommitments[0].ToHexString();
            _logger.LogIfDebug(() => $"[{_inherenceService.AccountId}]: {nameof(RegisterPerson)}, root commitment is {inherenceData.AssetRootCommitment.ToHexString()} and registration key is {registrationKey} for {nameof(biometricPersonData.SessionKey)}={biometricPersonData.SessionKey} obtained");

            string personGroupId = _portalConfiguration.DemoMode ? _portalConfiguration.FacePersonGroupId.ToLower() : inherenceData.Issuer.ToHexString().ToLower();

            string commitmentToRoot = inherenceData.AssetRootCommitment.ToHexString();

            if (!ConfidentialAssetsHelper.VerifySurjectionProof(inherenceData.RootRegistrationProof, inherenceData.AssetRootCommitment))
            {
                _logger.Error($"[{_inherenceService.AccountId}]: Registration Proofs failed for {nameof(biometricPersonData.SessionKey)}={biometricPersonData.SessionKey}");
                throw new InherenceRegistrationProofsIncorrectException();
            }

            if (inherenceData.AssociatedRootCommitment != null)
            {
                //=============================================================================
                // In the case when AssociatedRootCommitment is not null one of the following scenarios can occur:
                //   1. The user already has Root Inherence Protection attribute - in this case 
                //      image provided for registration must be checked for compliance with the existing factor
                //   2. The user does not have yet Root Protection attribute - in this case user must provide two images, 
                //      one for the Root Inherence protection attribute and the second for associated one. 
                //      Both images must be checked for matching and then two attributes will be created.
                //=============================================================================

                _logger.LogIfDebug(() => $"[{_inherenceService.AccountId}]: {nameof(RegisterPerson)}, {nameof(InherenceData)} with {nameof(biometricPersonData.SessionKey)}={biometricPersonData.SessionKey} contains {nameof(inherenceData.AssociatedRootCommitment)}");

                byte[] commitment = ConfidentialAssetsHelper.SumCommitments(inherenceData.AssetRootCommitment, inherenceData.AssociatedRootCommitment);

                if (!ConfidentialAssetsHelper.VerifySurjectionProof(inherenceData.AssociatedRegistrationProof, commitment))
                {
                    _logger.Error($"[{_inherenceService.AccountId}]: {nameof(inherenceData.AssociatedRegistrationProof)} failed for {nameof(biometricPersonData.SessionKey)}={biometricPersonData.SessionKey}");
                    return BadRequest(Resources.ERR_INHERENCE_REGISTRATION_PROOFS_INCORRECT);
                }

                string commitmentToAssociated = inherenceData.AssociatedRootCommitment.ToHexString();
                string associatedRegistrationKey = inherenceData.AssociatedRegistrationProof.AssetCommitments[0].ToHexString();
                _logger.LogIfDebug(() => $"[{_inherenceService.AccountId}]: {nameof(RegisterPerson)}, associated commitment is {commitmentToAssociated} and registration key is {associatedRegistrationKey} for {nameof(biometricPersonData.SessionKey)}={biometricPersonData.SessionKey} obtained");

                if (!biometricPersonData.Images.ContainsKey(commitmentToAssociated))
                {
                    _logger.Error($"[{_inherenceService.AccountId}]: {nameof(RegisterPerson)}, no image found for the associated commitment {commitmentToAssociated}");
                    return BadRequest($"No image found for the associated commitment {commitmentToAssociated}");
                }

                Guid rootRegistrationGuid = await GetPersonRegistrationGuid(personGroupId, registrationKey).ConfigureAwait(false);
                if (Guid.Empty.Equals(rootRegistrationGuid))
                {
                    _logger.LogIfDebug(() => $"[{_inherenceService.AccountId}]: {nameof(RegisterPerson)}, No root registration found for {nameof(biometricPersonData.SessionKey)}={biometricPersonData.SessionKey}");

                    if (!biometricPersonData.Images.ContainsKey(commitmentToRoot))
                    {
                        _logger.Error($"[{_inherenceService.AccountId}]: {nameof(RegisterPerson)}, no image found for the root commitment {commitmentToRoot}");
                        return BadRequest($"No image found for the root commitment {commitmentToRoot}");
                    }

                    imageContent = biometricPersonData.Images[commitmentToRoot];
                    byte[] faceAssociated = biometricPersonData.Images[commitmentToAssociated];

                    bool matches = await _facesService.VerifyFaces(imageContent, faceAssociated).ConfigureAwait(false);

                    if (!matches)
                    {
                        _logger.Error($"[{_inherenceService.AccountId}]: {nameof(RegisterPerson)}, faces do not match for {nameof(biometricPersonData.SessionKey)}={biometricPersonData.SessionKey}");
                        return BadRequest("Provided face images do not match one to another");
                    }

                    PersonFaceData rootFaceData = new PersonFaceData
                    {
                        PersonGroupId = personGroupId,
                        Name = registrationKey,
                        UserData = registrationKey,
                        ImageContent = imageContent
                    };

                    _logger.LogIfDebug(() => $"[{_inherenceService.AccountId}]: {nameof(RegisterPerson)}, adding root registration {registrationKey} for {nameof(biometricPersonData.SessionKey)}={biometricPersonData.SessionKey}");
                    await AddPerson(rootFaceData).ConfigureAwait(false);
                    registrationKey = inherenceData.AssociatedRegistrationProof.AssetCommitments[0].ToHexString();
                    imageContent = faceAssociated;

                    _logger.LogIfDebug(() => $"[{_inherenceService.AccountId}]: {nameof(RegisterPerson)}, proceeding with associated registration {registrationKey} for {nameof(biometricPersonData.SessionKey)}={biometricPersonData.SessionKey}");
                }
                else
                {
                    _logger.LogIfDebug(() => $"[{_inherenceService.AccountId}]: {nameof(RegisterPerson)}, Root registration with key {registrationKey} found for {nameof(biometricPersonData.SessionKey)}={biometricPersonData.SessionKey}");

                    registrationKey = inherenceData.AssociatedRegistrationProof.AssetCommitments[0].ToHexString();
                    imageContent = biometricPersonData.Images.ContainsKey(commitmentToAssociated) ? biometricPersonData.Images[commitmentToAssociated] : null;
                    bool isIdentical = await VerifyPersonFace(personGroupId, rootRegistrationGuid, imageContent).ConfigureAwait(false);

                    if (!isIdentical)
                    {
                        _logger.Error($"[{_inherenceService.AccountId}]: {nameof(RegisterPerson)}, face do not match to registered one for {nameof(biometricPersonData.SessionKey)}={biometricPersonData.SessionKey}");
                        throw new InherenceCrossMatchingFailedException(inherenceData.RootRegistrationProof.AssetCommitments[0].ToHexString());
                    }

                    _logger.LogIfDebug(() => $"[{_inherenceService.AccountId}]: {nameof(RegisterPerson)}, proceeding with associated registration {registrationKey} for {nameof(biometricPersonData.SessionKey)}={biometricPersonData.SessionKey}");
                }
            }
            else
            {
                _logger.LogIfDebug(() => $"[{_inherenceService.AccountId}]: {nameof(RegisterPerson)}, {nameof(InherenceData)} with {nameof(biometricPersonData.SessionKey)}={biometricPersonData.SessionKey} does not contain {nameof(inherenceData.AssociatedRootCommitment)}");
                if (!biometricPersonData.Images.ContainsKey(commitmentToRoot))
                {
                    _logger.Error($"[{_inherenceService.AccountId}]: {nameof(RegisterPerson)}, no image found for the root commitment {commitmentToRoot}");
                    return BadRequest($"No image found for the root commitment {commitmentToRoot}");
                }

                imageContent = biometricPersonData.Images[commitmentToRoot];
            }

            Guid guid = _dataAccessService.FindPersonGuid(registrationKey);

            PersonFaceData personFaceData = new PersonFaceData
            {
                PersonGroupId = personGroupId,
                Name = registrationKey,
                UserData = registrationKey,
                ImageContent = imageContent
            };

            try
            {
                if (guid == Guid.Empty)
                {
                    _logger.LogIfDebug(() => $"[{_inherenceService.AccountId}]: {nameof(RegisterPerson)}, adding registration {registrationKey} for {nameof(biometricPersonData.SessionKey)}={biometricPersonData.SessionKey}");
                    await AddPerson(personFaceData).ConfigureAwait(false);
                }
                else
                {
                    _logger.LogIfDebug(() => $"[{_inherenceService.AccountId}]: {nameof(RegisterPerson)}, replacing registration {registrationKey} for {nameof(biometricPersonData.SessionKey)}={biometricPersonData.SessionKey}");
                    await ReplacePerson(personFaceData).ConfigureAwait(false);
                }

            }
            catch (Exception ex)
            {
                if (ex is AggregateException aex)
                {
                    _logger.Error($"[{_inherenceService.AccountId}]: {nameof(RegisterPerson)} failed with aggregated exception", aex.InnerException);
                }
                else
                {
                    _logger.Error($"[{_inherenceService.AccountId}]: {nameof(RegisterPerson)} failed with exception", ex);
                }

                throw;
            }

            _logger.LogIfDebug(() => $"[{_inherenceService.AccountId}]: {nameof(RegisterPerson)}, completed successfully for {nameof(biometricPersonData.SessionKey)}={biometricPersonData.SessionKey}");

            return Ok();
        }

        private async Task<Guid> ReplacePerson(PersonFaceData personFaceData)
        {
            Guid guid = await _facesService.ReplacePersonFace(personFaceData).ConfigureAwait(false);

            if (guid != Guid.Empty)
            {
                _dataAccessService.UpdateBiometricRecord(personFaceData.UserData, guid);
            }
            else
            {
                throw new Exception("Replace person failed");
            }

            return guid;
        }

        private async Task<Guid> AddPerson(PersonFaceData personFaceData)
        {
            Guid guid = await _facesService.AddPerson(personFaceData).ConfigureAwait(false);

            if (guid != Guid.Empty)
            {
                _dataAccessService.AddBiometricRecord(personFaceData.UserData, guid);
            }
            else
            {
                throw new Exception("Adding person failed");
            }

            return guid;
        }

        [AllowAnonymous]
        [HttpPost("VerifyPersonFace")]
        public async Task<IActionResult> VerifyPersonFace([FromBody] BiometricVerificationDataDto verificationDataDto)
        {
            string personGroupId = _portalConfiguration.DemoMode ? _portalConfiguration.FacePersonGroupId.ToLower() : verificationDataDto.Issuer;
            byte[] imageContent = Convert.FromBase64String(verificationDataDto.ImageString);
            string registrationKey = verificationDataDto.RegistrationKey;

            bool isIdentical = await VerifyPersonFace(personGroupId, registrationKey, imageContent).ConfigureAwait(false);

            if (isIdentical)
            {
                Tuple<byte[], byte[]> signRes = _facesService.Sign(verificationDataDto.KeyImage.HexStringToByteArray());

                return Ok(new BiometricSignedVerificationDto { PublicKey = signRes.Item1.ToHexString(), Signature = signRes.Item2.ToHexString() });
            }

            return BadRequest();
        }

        private async Task<Guid> GetPersonRegistrationGuid(string personGroupId, string registrationKey)
        {
            _logger.Debug($"[{_inherenceService.AccountId}]: {nameof(GetPersonRegistrationGuid)}, {nameof(personGroupId)}={personGroupId}, {nameof(registrationKey)}={registrationKey}");
            Guid guid = _dataAccessService.FindPersonGuid(registrationKey);
            if (guid == Guid.Empty)
            {
                _logger.Debug($"[{_inherenceService.AccountId}]: {nameof(GetPersonRegistrationGuid)}, person Guid not found in DB, getting from FacesService...");
                Person person = (await _facesService.GetPersons(personGroupId).ConfigureAwait(false))
                    .FirstOrDefault(p => p.UserData.Equals(registrationKey, StringComparison.InvariantCultureIgnoreCase));
                if (person != null)
                {
                    guid = person.PersonId;
                }
            }

            return guid;
        }

        private async Task<bool> VerifyPersonFace(string personGroupId, string registrationKey, byte[] imageContent)
        {
            _logger.Debug($"[{_inherenceService.AccountId}]: {nameof(VerifyPersonFace)}, {nameof(personGroupId)}={personGroupId}, {nameof(registrationKey)}={registrationKey}");
            Guid guid = await GetPersonRegistrationGuid(personGroupId, registrationKey).ConfigureAwait(false);
            if (guid == Guid.Empty)
            {
                _logger.Error($"[{_inherenceService.AccountId}]: {nameof(VerifyPersonFace)}, not found person at group {personGroupId} with UserData = {registrationKey}");
                return false;
            }

            bool isIdentical = await VerifyPersonFace(personGroupId, guid, imageContent).ConfigureAwait(false);
            return isIdentical;
        }

        private async Task<bool> VerifyPersonFace(string personGroupId, Guid personGuid, byte[] imageContent)
        {
            _logger.Debug($"[{_inherenceService.AccountId}]: {nameof(VerifyPersonFace)}, {nameof(personGroupId)}={personGroupId}, {nameof(personGuid)}={personGuid}");

            (bool isIdentical, double confidence) = await _facesService.VerifyPerson(personGroupId, personGuid, imageContent).ConfigureAwait(false);
            return isIdentical;
        }

        [AllowAnonymous]
        [HttpPost("SignPersonFaceVerification")]
        public async Task<IActionResult> SignPersonFaceVerification([FromBody] BiometricPersonDataForSignatureDto biometricPersonData)
        {
            byte[] imageSource = Convert.FromBase64String(biometricPersonData.ImageSource);
            byte[] imageTarget = Convert.FromBase64String(biometricPersonData.ImageTarget);

            byte[] assetId = await _assetsService.GenerateAssetId(AttributesSchemes.ATTR_SCHEME_NAME_PASSPORTPHOTO, biometricPersonData.ImageSource, null).ConfigureAwait(false);
            byte[] sourceImageCommitment = biometricPersonData.SourceImageCommitment.HexStringToByteArray();

            SurjectionProof surjectionProof = new SurjectionProof
            {
                AssetCommitments = new byte[][] { biometricPersonData.SourceImageProofCommitment.HexStringToByteArray() },
                Rs = new BorromeanRingSignature
                {
                    E = biometricPersonData.SourceImageProofSignatureE.HexStringToByteArray(),
                    S = new byte[][] { biometricPersonData.SourceImageProofSignatureS.HexStringToByteArray() }
                }
            };

            if (!ConfidentialAssetsHelper.VerifyIssuanceSurjectionProof(surjectionProof, sourceImageCommitment, new byte[][] { assetId }))
            {
                return BadRequest("Surjection proofs validation failed");
            }

            //byte[] auxBytes = null; // Convert.FromBase64String(biometricPersonData.AuxMessage);

            //byte[] msg = new byte[sourceImageCommitment.Length + auxBytes?.Length ?? 0];

            //Array.Copy(sourceImageCommitment, 0, msg, 0, sourceImageCommitment.Length);

            //if ((auxBytes?.Length ?? 0) > 0)
            //{
            //	Array.Copy(auxBytes, 0, msg, sourceImageCommitment.Length, auxBytes.Length);
            //}

            bool res = await _facesService.VerifyFaces(imageSource, imageTarget).ConfigureAwait(false);

            if (res)
            {
                Tuple<byte[], byte[]> signRes = _facesService.Sign(sourceImageCommitment);

                return Ok(new BiometricSignedVerificationDto { PublicKey = signRes.Item1.ToHexString(), Signature = signRes.Item2.ToHexString() });
            }

            return BadRequest();
        }

        [HttpPost("CompareFaces")]
        public async Task<IActionResult> CompareFaces([FromBody] CompareFacesDto compareFaces)
        {
            byte[] face1 = Convert.FromBase64String(compareFaces.Face1);
            byte[] face2 = Convert.FromBase64String(compareFaces.Face2);
            bool isIdentical = await _facesService.VerifyFaces(face1, face2).ConfigureAwait(false);

            return Ok(isIdentical);
        }

        [HttpGet("Persons")]
        public async Task<IActionResult> GetAllPersons([FromQuery] string issuer)
        {
            string personGroupId = _portalConfiguration.DemoMode ? _portalConfiguration.FacePersonGroupId.ToLower() : issuer?.ToLower();
            IList<Person> people = await _facesService.GetPersons(personGroupId).ConfigureAwait(false);

            var persons = people.Select(p => new PersonDto
            {
                Name = p.Name,
                UserData = p.UserData,
                PersonId = p.PersonId,
                PersistedFaceIds = new List<Guid>(p.PersistedFaceIds)
            });

            return Ok(persons);
        }
    }
}
