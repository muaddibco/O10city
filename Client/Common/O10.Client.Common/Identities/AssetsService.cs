using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O10.Client.Common.Entities;
using O10.Client.Common.Exceptions;
using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.AttributesScheme;
using O10.Core;
using O10.Core.Architecture;
using O10.Core.HashCalculations;
using O10.Core.Identity;
using O10.Crypto.ConfidentialAssets;

namespace O10.Client.Common.Identities
{
    [RegisterDefaultImplementation(typeof(IAssetsService), Lifetime = LifetimeManagement.Singleton)]
	public class AssetsService : IAssetsService
	{
		private readonly IHashCalculation _hashCalculation;
		private readonly ISchemeResolverService _schemeResolverService;

        public AssetsService(IHashCalculationsRepository hashCalculationsRepository,
                             ISchemeResolverService schemeResolverService)
		{
			_hashCalculation = hashCalculationsRepository.Create(Globals.ASSET_CREATION_HASH_TYPE);
			_schemeResolverService = schemeResolverService;
        }

		public async Task<byte[]> GenerateAssetId(string schemeName, string attributeContent, string issuer, string miscName = null)
        {
            byte[] assetId = new byte[32];

            long schemeId = await GetSchemeId(schemeName, issuer).ConfigureAwait(false);

            byte[] hash = _hashCalculation.CalculateHash(Encoding.ASCII.GetBytes(attributeContent));
            Array.Copy(hash, 0, assetId, 0, hash.Length);
            Array.Copy(BitConverter.GetBytes(schemeId), 0, assetId, hash.Length, sizeof(long));

            return assetId;
        }

		public byte[] GenerateAssetId(long schemeId, string attributeContent)
		{
			byte[] assetId = new byte[32];

			byte[] hash = _hashCalculation.CalculateHash(Encoding.ASCII.GetBytes(attributeContent));
			Array.Copy(hash, 0, assetId, 0, hash.Length);
			Array.Copy(BitConverter.GetBytes(schemeId), 0, assetId, hash.Length, sizeof(long));

			return assetId;
		}

        public byte[] GenerateAssetCommitment(long schemeId, string attributeContent)
        {
            byte[] assetId = GenerateAssetId(schemeId, attributeContent);
            byte[] assetCommitment = CryptoHelper.GetNonblindedAssetCommitment(assetId);
            return assetCommitment;
        }


        public async Task<long> GetSchemeId(string schemeName, string issuer)
        {
            switch (schemeName)
            {
                case AttributesSchemes.ATTR_SCHEME_NAME_PASSPORTPHOTO:
                    return -1;
                case AttributesSchemes.ATTR_SCHEME_NAME_EMPLOYEEGROUP:
                    return -2;
                default:
                    AttributeDefinition attributeScheme = await _schemeResolverService.ResolveAttributeScheme(issuer, schemeName).ConfigureAwait(false);
                    if(attributeScheme == null)
                    {
                        throw new NoSchemeDefinedException(schemeName, issuer);
                    }
                    long schemeId = attributeScheme.SchemeId;
                    return schemeId;
            }
        }

        public async Task<string> GetAttributeSchemeName(byte[] assetId, string issuer)
		{
			long schemeId = BitConverter.ToInt64(assetId, 24);

            AttributeDefinition attributeScheme = await _schemeResolverService.ResolveAttributeScheme(issuer, schemeId).ConfigureAwait(false);

			return attributeScheme.SchemeName;
		}

        public async Task<AttributeDefinition> GetAttributeDefinition(byte[] assetId, string issuer)
        {
            long schemeId = BitConverter.ToInt64(assetId, 24);

            return await _schemeResolverService.ResolveAttributeScheme(issuer, schemeId).ConfigureAwait(false);
        }

        public void GetBlindingPoint(byte[] bindingKey, byte[] rootAssetId, out byte[] blindingPoint, out byte[] blindingFactor)
        {
            byte[] blindingFactorSeed = CryptoHelper.FastHash256(bindingKey, rootAssetId);
            blindingFactor = CryptoHelper.ReduceScalar32(blindingFactorSeed);
            blindingPoint = CryptoHelper.GetPublicKey(blindingFactor);
        }

        public void GetBlindingPoint(byte[] bindingKey, byte[] rootAssetId, byte[] assetId, out byte[] blindingPoint, out byte[] blindingFactor)
        {
            byte[] blindingFactorSeed = CryptoHelper.FastHash256(bindingKey, rootAssetId, assetId);
            blindingFactor = CryptoHelper.ReduceScalar32(blindingFactorSeed);
            blindingPoint = CryptoHelper.GetPublicKey(blindingFactor);
        }

        public byte[] GetBlindingPoint(params byte[][] scalars)
        {
            byte[] blindingFactorSeed = CryptoHelper.FastHash256(scalars.Select(s => new Memory<byte>(s)).ToArray());
            byte[] blindingFactor = CryptoHelper.ReduceScalar32(blindingFactorSeed);
            byte[] blindingPoint = CryptoHelper.GetPublicKey(blindingFactor);

            return blindingPoint;
        }

        public (byte[] blindingFactor, byte[] blindingPoint) GetBlindingFactorAndPoint(params byte[][] scalars)
        {
            byte[] blindingFactorSeed = CryptoHelper.FastHash256(scalars.Select(s => new Memory<byte>(s)).ToArray());
            byte[] blindingFactor = CryptoHelper.ReduceScalar32(blindingFactorSeed);
            byte[] blindingPoint = CryptoHelper.GetPublicKey(blindingFactor);

            return (blindingFactor, blindingPoint);
        }

        public byte[] GetBlindingFactor(params byte[][] scalars)
        {
            byte[] blindingFactorSeed = CryptoHelper.FastHash256(scalars.Select(s => new Memory<byte>(s)).ToArray());
            byte[] blindingFactor = CryptoHelper.ReduceScalar32(blindingFactorSeed);

            return blindingFactor;
        }

        public byte[] GetCommitmentBlindedByPoint(byte[] assetId, byte[] blindingPoint)
        {
            byte[] nonBlindedCommitment = CryptoHelper.GetNonblindedAssetCommitment(assetId);
            byte[] blindedCommitment = CryptoHelper.SumCommitments(nonBlindedCommitment, blindingPoint);

            return blindedCommitment;
        }

        public async Task<AttributeDefinition> GetRootAttributeDefinition(string issuer)
        {
            return await _schemeResolverService.GetRootAttributeScheme(issuer).ConfigureAwait(false);
        }

        public async Task<AttributeDefinition> GetRootAttributeDefinition(IKey issuer)
        {
            if (issuer is null)
            {
                throw new ArgumentNullException(nameof(issuer));
            }

            return await GetRootAttributeDefinition(issuer.ToString()).ConfigureAwait(false);
        }

        public async Task<IEnumerable<AttributeDefinition>> GetAssociatedAttributeDefinitions(string issuer)
        {
            var attributeSchemes = await _schemeResolverService.ResolveAttributeSchemes(issuer).ConfigureAwait(false);

            return attributeSchemes.Where(a => !a.IsRoot && AttributesSchemes.ATTR_SCHEME_NAME_PASSWORD != a.SchemeName);
        }

        public async Task<IEnumerable<AttributeDefinition>> GetAssociatedAttributeDefinitions(IKey issuer)
        {
            if (issuer is null)
            {
                throw new ArgumentNullException(nameof(issuer));
            }

            return await GetAssociatedAttributeDefinitions(issuer.ToString()).ConfigureAwait(false);
        }
    }
}
