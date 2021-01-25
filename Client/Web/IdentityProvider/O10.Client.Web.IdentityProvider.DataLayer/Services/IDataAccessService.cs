using O10.Core.Architecture;
using O10.IdentityProvider.DataLayer.Model;
using O10.IdentityProvider.DataLayer.Model.Enums;

namespace O10.IdentityProvider.DataLayer.Services
{
	[ServiceContract]
	public interface IDataAccessService
	{
        void Initialize();

		bool DoesAssetExist(string assetId);

		long AddIssuanceBlindingFactors(string issuanceBlindingFactor, string biometricBlindingFactor);

		BlindingFactorsRecord GetBlindingFactors(long blindingFactorsId);

		long AddAssetRegistrationSession(string sessionKey, string sessionCommitment);

        long AddAssetRegistration(string assetId, string issuanceCommitment, string biometricCommitment, string protectionCommitment, long issuanceBlindingRecordId);

		UserRecord GetAssetRegistration(string assetId);

		UserRecord RemoveAssetRegistration(string assetId);

        long AddBiometricRecord(long userRecordId, BiometricRecordType recordType, byte[] content);

        BiometricRecord GetBiometricRecord(long userRecordId, BiometricRecordType recordType);

		RegistrationSession GetAssetRegistrationSession(string sessionKey, string sessionCommitment);
		void RemoveAssetRegistrationSession(long registrationSessionId);

		void SetIdentityProviderAccountId(long accountId);

		void ClearIdentityProviderAccountId();

		long GetIdentityProviderAccountId();
	}
}
