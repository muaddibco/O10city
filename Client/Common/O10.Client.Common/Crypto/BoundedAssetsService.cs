using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using O10.Client.Common.Dtos.UniversalProofs;
using O10.Client.Common.Entities;
using O10.Client.Common.Exceptions;
using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.AttributesScheme;
using O10.Client.DataLayer.Model;
using O10.Core;
using O10.Core.Architecture;
using O10.Core.Cryptography;
using O10.Core.ExtensionMethods;
using O10.Core.Identity;
using O10.Crypto.ConfidentialAssets;

namespace O10.Client.Common.Crypto
{
    [RegisterDefaultImplementation(typeof(IBoundedAssetsService), Lifetime = LifetimeManagement.Scoped)]
	public class BoundedAssetsService : IBoundedAssetsService
	{
        private TaskCompletionSource<byte[]> _bindingKeySource;
        private readonly IAssetsService _assetsService;
        private readonly IEligibilityProofsProvider _eligibilityProofsProvider;
        private readonly IIdentityKeyProvider _identityKeyProvider;

        public BoundedAssetsService(
            IAssetsService assetsService, 
            IIdentityKeyProvidersRegistry identityKeyProvidersRegistry,
            IEligibilityProofsProvider eligibilityProofsProvider)
        {
            if (identityKeyProvidersRegistry is null)
            {
                throw new ArgumentNullException(nameof(identityKeyProvidersRegistry));
            }

            _assetsService = assetsService;
            _eligibilityProofsProvider = eligibilityProofsProvider;
            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
        }

        public async Task<SurjectionProof> CreateProofToRegistration(byte[] receiverPublicKey, byte[] blindingFactor, byte[] assetCommitment, params Memory<byte>[] assetIds)
        {
            (byte[] registrationBlindingFactor, byte[] registrationCommitment) = await GetBoundedCommitment(receiverPublicKey, assetIds).ConfigureAwait(false);
            byte[] blindingFactorToRegistration = ConfidentialAssetsHelper.GetDifferentialBlindingFactor(blindingFactor, registrationBlindingFactor);

            return ConfidentialAssetsHelper.CreateSurjectionProof(assetCommitment, new byte[][] { registrationCommitment }, 0, blindingFactorToRegistration);
        }

        public void GetBoundedCommitment(Memory<byte> assetId, Memory<byte> receiverPublicKey, out byte[] blindingFactor, out byte[] assetCommitment, params byte[][] scalars)
        {
            if(_bindingKeySource == null)
            {
                throw new BindingKeyNotInitializedException();
            }

            byte[] bindingKey = AsyncUtil.RunSync(async () => await _bindingKeySource.Task.ConfigureAwait(false));

            byte[] sk;

            if ((scalars?.Length ?? 0) > 0)
            {
                sk = ConfidentialAssetsHelper.SumScalars(scalars);
                sk = ConfidentialAssetsHelper.SumScalars(sk, bindingKey);
            }
            else
            {
                sk = bindingKey;
            }

            blindingFactor = ConfidentialAssetsHelper.GetReducedSharedSecret(sk, receiverPublicKey.Span);
            assetCommitment = ConfidentialAssetsHelper.GetAssetCommitment(blindingFactor, assetId);
        }

        public async Task<(byte[] blindingFactor, byte[] boundedCommitment)> GetBoundedCommitment(Memory<byte> receiverPublicKey, params Memory<byte>[] assetIds)
        {
            byte[] bindingKey = await _bindingKeySource.Task.ConfigureAwait(false);
            byte[] blindingFactor = ConfidentialAssetsHelper.GetReducedSharedSecret(bindingKey, receiverPublicKey.Span);
            byte[] boundedCommitment = ConfidentialAssetsHelper.GetAssetCommitment(blindingFactor, assetIds);

            return (blindingFactor, boundedCommitment);
        }

        public void Initialize(TaskCompletionSource<byte[]> bindingKeySource) => _bindingKeySource = bindingKeySource;

        public async Task Initialize(string pwd, bool replace = true)
        {
            await Task.Delay(50).ConfigureAwait(false);

            if (replace)
            {
                if (_bindingKeySource == null || _bindingKeySource.Task.IsCompleted)
                {
                    _bindingKeySource = new TaskCompletionSource<byte[]>();
                }

                _bindingKeySource.SetResult(GetBindingKey(pwd));
            }
            else if(!_bindingKeySource.Task.IsCompleted)
            {
                _bindingKeySource.SetResult(GetBindingKey(pwd));
            }
        }

        public bool IsBindingKeySet()
        {
            return _bindingKeySource.Task.IsCompleted;
        }

        public byte[] GetBindingKey(string pwd)
        {
            byte[] pwdHash = ConfidentialAssetsHelper.PasswordHash(pwd);

            return pwdHash;
        }

        public async Task<byte[]> GetBindingKey()
        {
            return await _bindingKeySource.Task.ConfigureAwait(false);
        }

        public async Task<AttributeProofs> GetRootAttributeProofs(byte[] bf, UserRootAttribute rootAttribute)
        {
            if (rootAttribute is null)
            {
                throw new ArgumentNullException(nameof(rootAttribute));
            }

            byte[] commitment = ConfidentialAssetsHelper.GetNonblindedAssetCommitment(rootAttribute.AssetId);
            byte[] commitmentToRoot = ConfidentialAssetsHelper.BlindAssetCommitment(commitment, bf);
            byte[] issuer = rootAttribute.Source.HexStringToByteArray();
            SurjectionProof eligibilityProof = await _eligibilityProofsProvider.CreateEligibilityProof(rootAttribute.OriginalCommitment, rootAttribute.OriginalBlindingFactor, commitmentToRoot, bf, issuer).ConfigureAwait(false);

            SurjectionProof proofToRegistration = await CreateProofToRegistration(issuer, bf, commitmentToRoot, rootAttribute.AssetId).ConfigureAwait(false);

            var attributeProofs = new AttributeProofs
            {
                SchemeName = rootAttribute.SchemeName,
                Commitment = _identityKeyProvider.GetKey(commitmentToRoot),
                BindingProof = eligibilityProof,
                CommitmentProof = new CommitmentProof
                {
                    SurjectionProof = proofToRegistration
                }
            };

            bool res = ConfidentialAssetsHelper.VerifySurjectionProof(attributeProofs.BindingProof, attributeProofs.Commitment.Value.Span);

            return attributeProofs;
        }

        public async Task<AttributeProofs> GetAssociatedAttributeProofs(BlindingAssetInput assetInput, BlindingAssetInput parentAssetInput, string attributeSchemeName)
        {
            if (assetInput is null)
            {
                throw new ArgumentNullException(nameof(assetInput));
            }

            if (parentAssetInput is null)
            {
                throw new ArgumentNullException(nameof(parentAssetInput));
            }

            byte[] commitmentToParentNB = ConfidentialAssetsHelper.GetNonblindedAssetCommitment(parentAssetInput.AssetId);
            byte[] commitmentToParent = ConfidentialAssetsHelper.BlindAssetCommitment(commitmentToParentNB, parentAssetInput.BlindingFactor);

            byte[] bindingKey = await GetBindingKey().ConfigureAwait(false);
            (byte[] bfToParent, byte[] blindingPointParent) = _assetsService.GetBlindingFactorAndPoint(bindingKey, parentAssetInput.AssetId);
            (byte[] bfValueBounded, byte[] blindingPointValue) = _assetsService.GetBlindingFactorAndPoint(bindingKey, parentAssetInput.AssetId, assetInput.AssetId);
            
            byte[] commitmentToValueNB = ConfidentialAssetsHelper.GetNonblindedAssetCommitment(assetInput.AssetId);
            
            byte[] commitmentToValueBounded = ConfidentialAssetsHelper.SumCommitments(blindingPointValue, commitmentToValueNB);
            byte[] commitmentToRootBinding = ConfidentialAssetsHelper.SumCommitments(blindingPointParent, commitmentToParentNB);
            commitmentToRootBinding = ConfidentialAssetsHelper.SumCommitments(commitmentToRootBinding, commitmentToValueNB);

            byte[] commitmentToValue = ConfidentialAssetsHelper.BlindAssetCommitment(commitmentToValueNB, assetInput.BlindingFactor);
            byte[] bfToValueDiff = ConfidentialAssetsHelper.GetDifferentialBlindingFactor(assetInput.BlindingFactor, bfValueBounded);
            var surjectionProofValue = ConfidentialAssetsHelper.CreateSurjectionProof(commitmentToValue, new byte[][] { commitmentToValueBounded }, 0, bfToValueDiff);
            
            byte[] commitmentToRootAndBinding = ConfidentialAssetsHelper.SumCommitments(commitmentToValue, commitmentToParent);
            byte[] bfProtectionAndRoot = ConfidentialAssetsHelper.SumScalars(assetInput.BlindingFactor, parentAssetInput.BlindingFactor);
            byte[] bfToRootDiff = ConfidentialAssetsHelper.GetDifferentialBlindingFactor(bfProtectionAndRoot, bfToParent);
            var surjectionProofRoot = ConfidentialAssetsHelper.CreateSurjectionProof(commitmentToRootAndBinding, new byte[][] { commitmentToRootBinding }, 0, bfToRootDiff);

            AttributeProofs associatedAttribute = new AttributeProofs
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

        public async Task<AttributeProofs> GetProtectionAttributeProofs(BlindingAssetInput rootAssetInput, string issuer)
        {
            if (rootAssetInput is null)
            {
                throw new ArgumentNullException(nameof(rootAssetInput));
            }

            if (issuer is null)
            {
                throw new ArgumentNullException(nameof(issuer));
            }

            var protectionAssetInput = new BlindingAssetInput
            {
                AssetId = await _assetsService.GenerateAssetId(AttributesSchemes.ATTR_SCHEME_NAME_PASSWORD, rootAssetInput.AssetId.ToHexString(), issuer).ConfigureAwait(false),
                BlindingFactor = ConfidentialAssetsHelper.GetRandomSeed()
            };

            return await GetAssociatedAttributeProofs(protectionAssetInput, rootAssetInput, AttributesSchemes.ATTR_SCHEME_NAME_PASSWORD).ConfigureAwait(false);
        }

        public async Task<SurjectionProof> GenerateBindingProofToRoot(BlindingAssetInput assetInput, BlindingAssetInput parentAssetInput)
        {
            if (assetInput is null)
            {
                throw new ArgumentNullException(nameof(assetInput));
            }

            if (parentAssetInput is null)
            {
                throw new ArgumentNullException(nameof(parentAssetInput));
            }

            byte[] commitment = ConfidentialAssetsHelper.GetNonblindedAssetCommitment(parentAssetInput.AssetId);
            byte[] commitmentToRoot = ConfidentialAssetsHelper.BlindAssetCommitment(commitment, parentAssetInput.BlindingFactor);

            byte[] bindingKey = await GetBindingKey().ConfigureAwait(false);
            byte[] bfRoot = _assetsService.GetBlindingFactor(bindingKey, parentAssetInput.AssetId);
            byte[] blindingPointRoot = _assetsService.GetBlindingPoint(bindingKey, parentAssetInput.AssetId);
            byte[] nonBlindedRootCommitment = ConfidentialAssetsHelper.GetNonblindedAssetCommitment(parentAssetInput.AssetId);
            byte[] nonBlindedAttributeCommitment = ConfidentialAssetsHelper.GetNonblindedAssetCommitment(assetInput.AssetId);
            byte[] commitmentToRootBinding = ConfidentialAssetsHelper.SumCommitments(blindingPointRoot, nonBlindedRootCommitment);
            commitmentToRootBinding = ConfidentialAssetsHelper.SumCommitments(commitmentToRootBinding, nonBlindedAttributeCommitment);

            byte[] blindedAttributeCommitment = ConfidentialAssetsHelper.BlindAssetCommitment(nonBlindedAttributeCommitment, assetInput.BlindingFactor);
            byte[] commitmentToRootAndBinding = ConfidentialAssetsHelper.SumCommitments(blindedAttributeCommitment, commitmentToRoot);
            byte[] bfProtectionAndRoot = ConfidentialAssetsHelper.SumScalars(assetInput.BlindingFactor, parentAssetInput.BlindingFactor);
            byte[] bfToRootDiff = ConfidentialAssetsHelper.GetDifferentialBlindingFactor(bfProtectionAndRoot, bfRoot);
            var surjectionProofRoot = ConfidentialAssetsHelper.CreateSurjectionProof(commitmentToRootAndBinding, new byte[][] { commitmentToRootBinding }, 0, bfToRootDiff);

            return surjectionProofRoot;
        }

        public async Task<RootIssuer> GetAttributeProofs(byte[] bf, UserRootAttribute rootAttribute, IEnumerable<UserAssociatedAttribute> associatedAttributes = null, bool withProtectionAttribute = false)
        {
            if (rootAttribute is null)
            {
                throw new ArgumentNullException(nameof(rootAttribute));
            }

            IKey issuerKeyRoot = _identityKeyProvider.GetKey(rootAttribute.Source.HexStringToByteArray());
            AttributesByIssuer rootAttributeByIssuer = new AttributesByIssuer
            {
                Issuer = issuerKeyRoot,
                RootAttribute = await GetRootAttributeProofs(bf, rootAttribute).ConfigureAwait(false)
            };

            // ================================================================================
            // Prepare proof of Password
            // ================================================================================
            if (withProtectionAttribute)
            {
                var protectionAssetInput = new BlindingAssetInput
                {
                    AssetId = rootAttribute.AssetId,
                    BlindingFactor = bf
                };

                var protectionAttribute = await GetProtectionAttributeProofs(protectionAssetInput, rootAttribute.Source).ConfigureAwait(false);
                rootAttributeByIssuer.Attributes.Add(protectionAttribute);
            }
            // ================================================================================

            var rootIssuer = new RootIssuer
            {
                Issuer = rootAttributeByIssuer.Issuer,
                IssuersAttributes = new List<AttributesByIssuer> { rootAttributeByIssuer }
            };

            var rootAssetInput = new BlindingAssetInput
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
                        byte[] attrBf = ConfidentialAssetsHelper.GetRandomSeed();
                        byte[] attrAssetId = await _assetsService.GenerateAssetId(attr.AttributeSchemeName, attr.Content, attr.Source).ConfigureAwait(false);

                        var childAssetInput = new BlindingAssetInput(attrAssetId);

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
