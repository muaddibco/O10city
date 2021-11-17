using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using O10.Client.Common.Dtos;
using O10.Client.Common.Dtos.UniversalProofs;
using O10.Client.Common.Exceptions;
using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.AttributesScheme;
using O10.Client.DataLayer.Model;
using O10.Core;
using O10.Core.Architecture;
using O10.Core.ExtensionMethods;
using O10.Core.Identity;
using O10.Crypto.ConfidentialAssets;
using O10.Crypto.Models;

namespace O10.Client.Stealth
{
    [RegisterDefaultImplementation(typeof(IBoundedAssetsService), Lifetime = LifetimeManagement.Scoped)]
    public class BoundedAssetsService : IBoundedAssetsService
    {
        private TaskCompletionSource<byte[]> _bindingKeySource;
        private readonly IAssetsService _assetsService;
        private readonly IStealthClientCryptoService _clientCryptoService;
        private readonly IEligibilityProofsProvider _eligibilityProofsProvider;
        private readonly IIdentityKeyProvider _identityKeyProvider;

        public BoundedAssetsService(
            IAssetsService assetsService,
            IStealthClientCryptoService clientCryptoService,
            IIdentityKeyProvidersRegistry identityKeyProvidersRegistry,
            IEligibilityProofsProvider eligibilityProofsProvider)
        {
            if (identityKeyProvidersRegistry is null)
            {
                throw new ArgumentNullException(nameof(identityKeyProvidersRegistry));
            }

            _assetsService = assetsService;
            _clientCryptoService = clientCryptoService;
            _eligibilityProofsProvider = eligibilityProofsProvider;
            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
        }

        // TODO: need to add the capability of obtaining set of registration commitments already existing at the receiver
        // TODO: this function must able to calculate Cr = Pb + Ir + Ig
        public async Task<SurjectionProof> CreateProofToRegistration(Memory<byte> receiverPublicKey, Memory<byte> blindingFactor, Memory<byte> assetCommitment, Memory<byte> rootAssetId, IEnumerable<Memory<byte>> assetIds = null)
        {
            /*if (assetIds is null)
            {
                throw new ArgumentNullException(nameof(assetIds));
            }

            if(!assetIds.Any())
            {
                throw new ArgumentException("There must be at least one assetId for proof of registration creation", nameof(assetIds));
            }*/

            // registrationBlindingFactor = scalar(H(pwd)*receiverPublicKey)
            // registrationCommitment = G * scalar(H(pwd)*receiverPublicKey) + I(rootAssetId)
            // or
            // registrationCommitment = G * H(scalar(H(pwd)*receiverPublicKey)|assetIds) + I(rootAssetId)
            (byte[] registrationBlindingFactor, byte[] registrationCommitment) = await GetBoundedCommitment(receiverPublicKey, rootAssetId, assetIds).ConfigureAwait(false);
            byte[] blindingFactorToRegistration = CryptoHelper.GetDifferentialBlindingFactor(blindingFactor.Span, registrationBlindingFactor);

            var sp = CryptoHelper.CreateSurjectionProof(assetCommitment.Span, new byte[][] { registrationCommitment }, 0, blindingFactorToRegistration);

            /// TODO: ??? what is this ???
            if (assetIds?.Count() > 1)
            {
                sp.AssetCommitments[0] = CryptoHelper.AddAssetIds(registrationCommitment, assetIds.Skip(1));
            }

            return sp;
        }

        public void GetBoundedCommitment(Memory<byte> assetId, Memory<byte> receiverPublicKey, out byte[] blindingFactor, out byte[] assetCommitment, params byte[][] scalars)
        {
            if (_bindingKeySource == null)
            {
                throw new BindingKeyNotInitializedException();
            }

            // TODO: change to async invocation
            byte[] bindingKey = AsyncUtil.RunSync(async () => await _bindingKeySource.Task.ConfigureAwait(false));

            byte[] sk;

            if ((scalars?.Length ?? 0) > 0)
            {
                sk = CryptoHelper.SumScalars(scalars);
                sk = CryptoHelper.SumScalars(sk, bindingKey);
            }
            else
            {
                sk = bindingKey;
            }

            blindingFactor = CryptoHelper.GetReducedSharedSecret(sk, receiverPublicKey.Span);
            assetCommitment = CryptoHelper.GetAssetCommitment(blindingFactor, assetId);
        }

        public async Task<(byte[] blindingFactor, byte[] boundedCommitment)> GetBoundedCommitment(Memory<byte> receiverPublicKey, Memory<byte> rootAssetId, IEnumerable<Memory<byte>> assetIds = null)
        {
            byte[] bindingKey = await _bindingKeySource.Task.ConfigureAwait(false);
            byte[] blindingFactor = CryptoHelper.GetReducedSharedSecret(bindingKey, receiverPublicKey.Span);
            byte[] boundedCommitment = CryptoHelper.GetAssetCommitment(blindingFactor.AsMemory().Join(assetIds), rootAssetId);

            return (blindingFactor, boundedCommitment);
        }

        public void Initialize(TaskCompletionSource<byte[]> bindingKeySource) => _bindingKeySource = bindingKeySource;

        public async Task Initialize(string pwd, bool replace = true)
        {
            await Task.Delay(50).ConfigureAwait(false);

            if (_bindingKeySource == null)
            {
                _bindingKeySource = new TaskCompletionSource<byte[]>();
            }

            if (replace)
            {
                if (_bindingKeySource.Task.IsCompleted)
                {
                    _bindingKeySource = new TaskCompletionSource<byte[]>();
                }

                _bindingKeySource.SetResult(GetBindingKey(pwd));
            }
            else
            {
                if (!_bindingKeySource.Task.IsCompleted)
                {
                    _bindingKeySource.SetResult(GetBindingKey(pwd));
                }
            }
        }

        public bool IsBindingKeySet()
        {
            return _bindingKeySource.Task.IsCompleted;
        }

        public byte[] GetBindingKey(string pwd)
        {
            byte[] pwdHash = CryptoHelper.PasswordHash(pwd);

            return pwdHash;
        }

        public async Task<byte[]> GetBindingKey()
        {
            return await _bindingKeySource.Task.ConfigureAwait(false);
        }

        public async Task<AttributeProofs> GetRootAttributeProofs(Memory<byte> bf, UserRootAttribute rootAttribute, IKey target = null, IEnumerable<Memory<byte>> assetIds = null)
        {
            if (rootAttribute is null)
            {
                throw new ArgumentNullException(nameof(rootAttribute));
            }

            byte[] commitment = CryptoHelper.GetNonblindedAssetCommitment(rootAttribute.AssetId);
            var commitmentToRoot = _identityKeyProvider.GetKey(CryptoHelper.BlindAssetCommitment(commitment, bf));
            byte[] issuer = rootAttribute.Source.HexStringToByteArray();
            byte[] issuanceBf = _clientCryptoService.GetBlindingFactor(rootAttribute.IssuanceTransactionKey);
            SurjectionProof eligibilityProof = await _eligibilityProofsProvider.CreateEligibilityProof(commitmentToRoot.Value, bf, rootAttribute.IssuanceCommitment, issuanceBf, issuer).ConfigureAwait(false);

            var attributeProofs = new AttributeProofs
            {
                SchemeName = rootAttribute.SchemeName,
                Commitment = commitmentToRoot,
                BindingProof = eligibilityProof,
                CommitmentProof = await GetCommitmentProof(bf, rootAttribute, target, assetIds, commitmentToRoot).ConfigureAwait(false)
            };

            bool res = CryptoHelper.VerifySurjectionProof(attributeProofs.BindingProof, attributeProofs.Commitment.Value.Span);

            return attributeProofs;

            async Task<CommitmentProof> GetCommitmentProof(Memory<byte> bf, UserRootAttribute rootAttribute, IKey target, IEnumerable<Memory<byte>> assetIds, IKey commitmentToRoot) =>
                target != null ? new CommitmentProof
                {
                    SurjectionProof = await CreateProofToRegistration(target.Value, bf, commitmentToRoot.Value, rootAttribute.AssetId, assetIds).ConfigureAwait(false)
                } : null;
        }

        public async Task<AttributeProofs> GetAssociatedAttributeProofs(BlindingAssetInputDTO assetInput, BlindingAssetInputDTO parentAssetInput, string attributeSchemeName, byte[] externalBindingKey = null)
        {
            if (assetInput is null)
            {
                throw new ArgumentNullException(nameof(assetInput));
            }

            if (parentAssetInput is null)
            {
                throw new ArgumentNullException(nameof(parentAssetInput));
            }

            byte[] commitmentToParentNB = CryptoHelper.GetNonblindedAssetCommitment(parentAssetInput.AssetId);
            byte[] commitmentToParent = CryptoHelper.BlindAssetCommitment(commitmentToParentNB, parentAssetInput.BlindingFactor);

            byte[] bindingKey = externalBindingKey ?? await GetBindingKey().ConfigureAwait(false);
            (byte[] bfToParent, byte[] blindingPointParent) = _assetsService.GetBlindingFactorAndPoint(bindingKey, parentAssetInput.AssetId);
            (byte[] bfValueBounded, byte[] blindingPointValue) = _assetsService.GetBlindingFactorAndPoint(bindingKey, parentAssetInput.AssetId, assetInput.AssetId);

            byte[] commitmentToValueNB = CryptoHelper.GetNonblindedAssetCommitment(assetInput.AssetId);

            byte[] commitmentToValueBounded = CryptoHelper.SumCommitments(blindingPointValue, commitmentToValueNB);
            byte[] commitmentToRootBinding = CryptoHelper.SumCommitments(blindingPointParent, commitmentToParentNB);
            commitmentToRootBinding = CryptoHelper.SumCommitments(commitmentToRootBinding, commitmentToValueNB);

            byte[] commitmentToValue = CryptoHelper.BlindAssetCommitment(commitmentToValueNB, assetInput.BlindingFactor);
            byte[] bfToValueDiff = CryptoHelper.GetDifferentialBlindingFactor(assetInput.BlindingFactor, bfValueBounded);
            var surjectionProofValue = CryptoHelper.CreateSurjectionProof(commitmentToValue, new byte[][] { commitmentToValueBounded }, 0, bfToValueDiff);

            byte[] commitmentToRootAndBinding = CryptoHelper.SumCommitments(commitmentToValue, commitmentToParent);
            byte[] bfProtectionAndRoot = CryptoHelper.SumScalars(assetInput.BlindingFactor, parentAssetInput.BlindingFactor);
            byte[] bfToRootDiff = CryptoHelper.GetDifferentialBlindingFactor(bfProtectionAndRoot, bfToParent);
            var surjectionProofRoot = CryptoHelper.CreateSurjectionProof(commitmentToRootAndBinding, new byte[][] { commitmentToRootBinding }, 0, bfToRootDiff);

            AttributeProofs associatedAttribute = new()
            {
                SchemeName = attributeSchemeName,
                Commitment = _identityKeyProvider.GetKey(commitmentToValue),
                BindingProof = surjectionProofRoot,
                CommitmentProof = new CommitmentProof
                {
                    SurjectionProof = surjectionProofValue
                }
            };

            return associatedAttribute;
        }

        public async Task<AttributeProofs> GetProtectionAttributeProofs(BlindingAssetInputDTO rootAssetInput, string issuer, byte[] externalBindingKey = null)
        {
            if (rootAssetInput is null)
            {
                throw new ArgumentNullException(nameof(rootAssetInput));
            }

            if (issuer is null)
            {
                throw new ArgumentNullException(nameof(issuer));
            }

            var protectionAssetInput = new BlindingAssetInputDTO
            {
                AssetId = await _assetsService.GenerateAssetId(AttributesSchemes.ATTR_SCHEME_NAME_PASSWORD, rootAssetInput.AssetId.ToHexString(), issuer).ConfigureAwait(false),
                BlindingFactor = CryptoHelper.GetRandomSeed()
            };

            return await GetAssociatedAttributeProofs(protectionAssetInput, rootAssetInput, AttributesSchemes.ATTR_SCHEME_NAME_PASSWORD, externalBindingKey).ConfigureAwait(false);
        }

        public async Task<SurjectionProof> GenerateBindingProofToRoot(BlindingAssetInputDTO assetInput, BlindingAssetInputDTO parentAssetInput)
        {
            if (assetInput is null)
            {
                throw new ArgumentNullException(nameof(assetInput));
            }

            if (parentAssetInput is null)
            {
                throw new ArgumentNullException(nameof(parentAssetInput));
            }

            byte[] commitment = CryptoHelper.GetNonblindedAssetCommitment(parentAssetInput.AssetId);
            byte[] commitmentToRoot = CryptoHelper.BlindAssetCommitment(commitment, parentAssetInput.BlindingFactor);

            byte[] bindingKey = await GetBindingKey().ConfigureAwait(false);
            byte[] bfRoot = _assetsService.GetBlindingFactor(bindingKey, parentAssetInput.AssetId);
            byte[] blindingPointRoot = _assetsService.GetBlindingPoint(bindingKey, parentAssetInput.AssetId);
            byte[] nonBlindedRootCommitment = CryptoHelper.GetNonblindedAssetCommitment(parentAssetInput.AssetId);
            byte[] nonBlindedAttributeCommitment = CryptoHelper.GetNonblindedAssetCommitment(assetInput.AssetId);
            byte[] commitmentToRootBinding = CryptoHelper.SumCommitments(blindingPointRoot, nonBlindedRootCommitment);
            commitmentToRootBinding = CryptoHelper.SumCommitments(commitmentToRootBinding, nonBlindedAttributeCommitment);

            byte[] blindedAttributeCommitment = CryptoHelper.BlindAssetCommitment(nonBlindedAttributeCommitment, assetInput.BlindingFactor);
            byte[] commitmentToRootAndBinding = CryptoHelper.SumCommitments(blindedAttributeCommitment, commitmentToRoot);
            byte[] bfProtectionAndRoot = CryptoHelper.SumScalars(assetInput.BlindingFactor, parentAssetInput.BlindingFactor);
            byte[] bfToRootDiff = CryptoHelper.GetDifferentialBlindingFactor(bfProtectionAndRoot, bfRoot);
            var surjectionProofRoot = CryptoHelper.CreateSurjectionProof(commitmentToRootAndBinding, new byte[][] { commitmentToRootBinding }, 0, bfToRootDiff);

            return surjectionProofRoot;
        }

        public async Task<RootIssuer> GetAttributeProofs(byte[] bf, UserRootAttribute rootAttribute, IKey target = null, IEnumerable<UserAssociatedAttribute> associatedAttributes = null, byte[] externalBindingKey = null)
        {
            if (rootAttribute is null)
            {
                throw new ArgumentNullException(nameof(rootAttribute));
            }

            IKey issuerKeyRoot = _identityKeyProvider.GetKey(rootAttribute.Source.HexStringToByteArray());
            AttributesByIssuer rootAttributeByIssuer = new()
            {
                Issuer = issuerKeyRoot,
                RootAttribute = await GetRootAttributeProofs(bf, rootAttribute, target).ConfigureAwait(false)
            };

            // ================================================================================
            // Prepare proof of Password
            // ================================================================================
            if (externalBindingKey != null)
            {
                var protectionAssetInput = new BlindingAssetInputDTO
                {
                    AssetId = rootAttribute.AssetId,
                    BlindingFactor = bf
                };

                var protectionAttribute = await GetProtectionAttributeProofs(protectionAssetInput, rootAttribute.Source, externalBindingKey).ConfigureAwait(false);
                rootAttributeByIssuer.Attributes.Add(protectionAttribute);
            }
            // ================================================================================

            var rootIssuer = new RootIssuer
            {
                Issuer = rootAttributeByIssuer.Issuer,
                IssuersAttributes = new List<AttributesByIssuer> { rootAttributeByIssuer }
            };

            var rootAssetInput = new BlindingAssetInputDTO
            {
                AssetId = rootAttribute.AssetId,
                BlindingFactor = bf
            };

            if (associatedAttributes?.Any() ?? false)
            {
                var attrsByIssuers = associatedAttributes.GroupBy(a => a.Source);
                foreach (var attrsByIssuer in attrsByIssuers)
                {
                    IKey issuerKey = _identityKeyProvider.GetKey(attrsByIssuer.Key.HexStringToByteArray());
                    var rootAttrDefinition = await _assetsService.GetRootAttributeDefinition(issuerKey).ConfigureAwait(false);
                    foreach (var attr in attrsByIssuer)
                    {
                        byte[] attrBf = CryptoHelper.GetRandomSeed();
                        byte[] attrAssetId = await _assetsService.GenerateAssetId(attr.AttributeSchemeName, attr.Content, attr.Source).ConfigureAwait(false);

                        var childAssetInput = new BlindingAssetInputDTO(attrAssetId);

                        var attrProofs = await GetAssociatedAttributeProofs(childAssetInput, rootAssetInput, attr.AttributeSchemeName).ConfigureAwait(false);

                        var attrs = rootIssuer.IssuersAttributes.Find(i => i.Issuer.Equals(issuerKey));
                        if (attrs == null)
                        {
                            attrs = new AttributesByIssuer(issuerKey);
                            rootIssuer.IssuersAttributes.Add(attrs);
                        }

                        if (attr.AttributeSchemeName == rootAttrDefinition.SchemeName)
                        {
                            attrs.RootAttribute = attrProofs;
                        }
                        else
                        {
                            attrs.Attributes.Add(attrProofs);
                        }
                    }
                }
            }

            return rootIssuer;
        }
    }
}
