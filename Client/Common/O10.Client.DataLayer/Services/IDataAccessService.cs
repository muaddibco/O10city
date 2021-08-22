using System;
using System.Collections.Generic;
using O10.Client.DataLayer.ElectionCommittee;
using O10.Client.DataLayer.Entities;
using O10.Client.DataLayer.Enums;
using O10.Client.DataLayer.Model;
using O10.Client.DataLayer.Model.ConsentManagement;
using O10.Client.DataLayer.Model.Inherence;
using O10.Client.DataLayer.Model.Scenarios;
using O10.Client.DataLayer.Model.ServiceProviders;
using O10.Client.DataLayer.Model.Users;
using O10.Core.Architecture;
using O10.Core.Identity;

namespace O10.Client.DataLayer.Services
{
    [ServiceContract]
	public interface IDataAccessService
	{
		bool Initialize();

		#region Identity

		Identity CreateIdentity(long accountId, string description, (string attrName, string content)[] attributes);

		Identity GetIdentity(long id);

		void DuplicateAssociatedAttributes(long oldAccountId, long newAccountId);

		void UpdateIdentityAttributeCommitment(long identityAttributeId, IKey commitment);

		Identity GetIdentityByAttribute(long accountId, string attributeName, string attributeValue);

		IEnumerable<Identity> GetIdentities(long accountId);

		long AddOrUpdateIdentityTarget(long identityId, string publicSpendKey, string publicViewKey);
		IdentityTarget GetIdentityTarget(long identityId);

		#endregion Identity

		#region User

		IEnumerable<UserRootAttribute> GetUserAttributes(long accountId);

		bool RemoveUserAttribute(long accountId, long userAttributeId);

		long AddNonConfirmedRootAttribute(long accountId, string content, string issuer, string schemeName, byte[] assetId);

		List<UserRootAttribute> GetAllNonConfirmedRootAttributes(long accountId);

		void UpdateConfirmedRootAttribute(UserRootAttribute userRootAttribute);

		bool DeleteNonConfirmedUserRootAttribute(long accountId, string content);

		UserRootAttribute GetUserRootAttribute(long rootAttributeId);
		UserRootAttribute GetRootAttributeByOriginalCommitment(long accountId, IKey originalCommitment);

		long AddUserRootAttribute(long accountId, UserRootAttribute attribute);

		bool UpdateUserAttribute(long accountId, string oldKeyImage, string keyImage, IKey lastCommitment, IKey lastTransactionKey, IKey lastDestinationKey);

		bool UpdateUserAttributeContent(long userAttributeId, string content);

		void UpdateUserAttributeContent(long accountId, byte[] originalCommitment, string content);

		List<long> MarkUserRootAttributesOverriden(long accountId, byte[] issuanceCommitment);
		long MarkUserRootAttributesOverriden2(long accountId, byte[] originalCommitment);

		UserSettings GetUserSettings(long accountId);

		void SetUserSettings(long accountId, UserSettings userSettings);

		long AddUserRegistration(long accountId, string commitment, string spInfo, string assetId, string issuer);
		void RemoveUserRegistration(long registrationId);
		void RemoveUserRegistration(long accountId, string commitment);

		List<UserRegistration> GetUserRegistrations(long accountId);

		IEnumerable<UserAssociatedAttribute> GetUserAssociatedAttributes(long accountId);

		void UpdateUserAssociatedAttributes(long accountId, string issuer, IEnumerable<Tuple<string, string>> associatedAttributes, byte[] rootAssetId = null);

		void AddOrUpdateUserIdentityIsser(string key, string issuerAlias, string description);

		string GetUserIdentityIsserAlias(string key);

		void StoreAssociatedAttributes(string rootIssuer, string rootAssetId, IEnumerable<AssociatedAttributeBackup> attributes);

		IEnumerable<AssociatedAttributeBackup> GetAssociatedAttributeBackups(string rootIssuer, string rootAssetId);

		void AddUserTransactionSecret(long accountId, string keyImage, string issuer, string assetId);

		UserTransactionSecret GetUserTransactionSecrets(long accountId, string keyImage);

		void RemoveUserTransactionSecret(long accountId, string keyImage);

		#endregion User

		#region Accounts

		string GetAccountKeyValue(long accountId, string key);
		Dictionary<string, string> GetAccountKeyValues(long accountId, params string[] filter);
		void SetAccountKeyValues(long accountId, Dictionary<string, string> keyValues);
		void RemoveAccountKeyValues(long accountId, IEnumerable<string> keys);
		List<Account> GetAccounts();

		Account GetAccount(long accountId);
		Account? FindAccountByAlias(string alias);
		Account GetAccount(byte[] publicKey);

		List<Account> GetAccountsByType(AccountType accountType);

		void SetAccountCompromised(long accountId);

		void ClearAccountCompromised(long accountId);

		long AddAccount(byte accountType, string accountInfo, byte[] secretSpendKeyEnc, byte[] secretViewKeyEnc, byte[] publicSpendKey, byte[] publicViewKey, long lastAggregatedRegistrations, bool isPrivate = false);
		void UpdateAccount(long accountId, string accountInfo, byte[] publicSpendKey, byte[] publicViewKey);
		void ResetAccount(long accountId, byte[] secretSpendKeyEnc, byte[] secretViewKeyEnc, byte[] publicSpendKey, byte[] publicViewKey, long lastAggregatedRegistrations);
		bool GetAccountId(byte[] publicKey, out long accountId);
		bool GetAccountId(byte[] publicSpendKey, byte[] publicViewKey, out long accountId);
		void RemoveAccount(long accountId);

		long DuplicateUserAccount(long accountId, string accountInfo);

		void OverrideUserAccount(long accountId, byte[] secretSpendKeyEnc, byte[] secretViewKeyEnc, byte[] publicSpendKey, byte[] publicViewKey, long lastAggregatedRegistrations);

		byte[] GetAesInitializationVector();

		#endregion Accounts

		#region Biometric

		byte[] GetBiometricSecretKey();

		void AddBiometricRecord(string userData, Guid personGuid);
		void UpdateBiometricRecord(string userData, Guid personGuid);

		Guid FindPersonGuid(string userData);

		bool RemoveBiometricPerson(string userData);

		#endregion Biometric

		#region Service Providers

		long AddServiceProviderRegistration(long accountId, byte[] commitment);

		bool GetServiceProviderRegistrationId(long accountId, byte[] commitment, out long serviceProviderRegistrationId);

		IEnumerable<ServiceProviderRegistration> GetServiceProviderRegistrations(long accountId);
		ServiceProviderRegistration GetServiceProviderRegistration(long accountId, byte[] registrationKey);
		ServiceProviderRegistration GetServiceProviderRegistration(long registrationId);

		void StoreSpAttribute(long accountId, string attributeSchemeName, byte[] assetId, string source, byte[] originalBlindingFactor, byte[] originalCommitment, byte[] issuingCommitment);

		IEnumerable<SpIdenitityValidation> GetSpIdenitityValidations(long accountId);

		void AdjustSpIdenitityValidations(long accountId, IEnumerable<SpIdenitityValidation> spIdenitityValidations);

        #region Relations

        long AddRelationGroup(long accountId, string groupName);
		void RemoveRelationGroup(long accountId, long groupId);
		IEnumerable<RelationGroup> GetRelationGroups(long accountId);
		string[] GetRelationGroupNames(long accountId);

		long AddRelationToGroup(long accountId, string description, string rootAttributeRaw, long groupId);

		void ChangeRelationGroup(long accountId, long relationId, long groupId);

		RelationRecord SetRelationRegistrationCommitment(long accountId, long relationId, string registrationCommitment);

		RelationRecord RemoveRelation(long accountId, long relationId);
		List<RelationRecord> GetRelationRecords(long accountId, string attributeContent);
		List<RelationRecord> GetRelationRecords(long accountId);
		IEnumerable<RelationRecord> GetRelations(long accountId, long groupId);
		IEnumerable<RelationRecord> GetRelations(long accountId);
		IEnumerable<RelationRecord> GetNotAssiginedRelations(long accountId);
		bool IsRelationExist(long accountId, string registrationCommitment);
        
		#endregion Relations

        IEnumerable<SpDocument> GetSpDocuments(long accountId);

		SignedDocumentEntity GetSpDocument(long accountId, string hash);
		SpDocument GetSpDocument(long accountId, long spDocumentId);

		long AddSpDocument(long accountId, string documentName, string hash);

		void UpdateSpDocumentChangeRecord(long accountId, string hash, ulong recordHeight);

		void RemoveSpDocument(long accountId, long spDocumentId);

		long AddSpDocumentAllowedSigner(long accountId, long spDocumentId, string groupOwner, string groupName, string groupCommitment, string blindingFactor);

		long RemoveSpDocumentAllowedSigner(long accountId, long spDocumentAllowedSignerId);

		long AddSpDocumentSignature(long accountId, long spDocumentId, ulong documentRecordHeight, ulong signatureRecordHeight);
		bool UpdateSpDocumentSignature(long accountId, string documentHash, ulong documentRecordHeight, ulong signatureRecordHeight, byte[] documentSignRecord);

		IEnumerable<SpDocumentSignature> GetSpDocumentSignatures(long accountId, long spDocumentId);

		IEnumerable<SpUserTransaction> GetSpUserTransactions(long accountId);

		long AddSpUserTransaction(long accountId, long registrationId, string transactionId, string description);

		bool SetSpUserTransactionConfirmed(long accountId, string transactionId);
		bool SetSpUserTransactionDeclined(long accountId, string transactionId);
		bool SetSpUserTransactionCompromised(long accountId, string transactionId);

		#endregion Service Providers

		bool GetLastUpdatedCombinedBlockHeight(long accountId, out long lastUpdatedCombinedBlockHeight);
		void StoreLastUpdatedCombinedBlockHeight(long accountId, long lastUpdatedCombinedBlockHeight);

		long AddAutoLogin(long accountId, byte[] secretKey);

		bool IsAutoLoginExist(long accountId);

		IEnumerable<AutoLogin> GetAutoLogins();
		long AddUserGroupRelation(long accountId, string groupOwnerName, string groupOwnerKey, string groupName, string assetId, string issuer);
		(string groupOwnerName, string issuer, string assetId) GetRelationUserAttributes(long accountId, string groupOwnerKey, string groupName);
		void RemoveUserGroupRelation(long userGroupRelationId);
		IEnumerable<UserGroupRelation> GetUserGroupRelations(long accountId);

		long AddGroupRelation(string groupOwnerKey, string groupName, string assetId, string issuer);
		IEnumerable<GroupRelation> GetGroupRelations(string assetId, string issuer);

		long AddRegistrationCommitment(string commitment, string description, string assetId, string issuer);
		IEnumerable<RegistrationCommitment> GetRegistrationCommitments(string assetId, string issuer);

		#region SAML

		IEnumerable<SamlIdentityProvider> GetSamlIdentityProviders();

		long SetSamlIdentityProvider(string entityId, string publicSpendKey, string secretViewKey);

		bool RemoveSamlIdentityProvider(string entityId);

		bool StoreSamlServiceProvider(string entityId, string singleLogoutUrl);

		SamlServiceProvider GetSamlServiceProvider(string entityId);

		void SetSamlSettings(long defaultSamlIdpId, long defaultSamlIdpAccountId);

		SamlSettings GetSamlSettings();

		#endregion SAML

		#region Identity Schemes

		long AddAttributeToScheme(string issuer, string attributeName, string attributeSchemeName, string alias, string description);

		IEnumerable<IdentitiesScheme> GetAttributesSchemeByIssuer(string issuer, bool activeOnly = false);

		void DeactivateAttribute(long identitiesSchemeId);
		void ActivateAttribute(long identitiesSchemeId);
		IdentitiesScheme GetRootIdentityScheme(string issuer);
		void ToggleOnRootAttributeScheme(long identitiesSchemeId);
		void ToggleOffRootAttributeSchemes(string issuer);

		#endregion Identity Schemes

		#region ConsentManagementSettings

		ConsentManagementSettings GetConsentManagementSettings();

		void SetConsentManagementSettings(ConsentManagementSettings consentManagementSettings);

		#endregion ConsentManagementSettings

		bool CheckAttributeSchemeToCommitmentMatching(string schemeName, string commitment);

		#region Scenarios

		IEnumerable<ScenarioSession> GetScenarioSessions(string userSubject);

		long AddNewScenarionSession(string userSubject, int scenarioId);

		void UpdateScenarioSessionStep(long scenarionSessionId, int step);

		void RemoveScenarioSession(string userSubject, int scenarioId);

		ScenarioSession GetScenarioSession(long scenarioSessionId);

		long AddScenarionSessionAccount(long scenarioSessionId, long accountId);

		IEnumerable<ScenarioAccount> GetScenarioAccounts(long scenarioSessionId);

		#endregion Scenarios

		#region Witnesses
		bool WitnessAsProcessed(long accountId, long witnessId);

		void RemoveAllWitnessed(long accountId);
		#endregion Witnesses

		#region Inherence

		InherenceSetting GetInherenceSetting(string name);

		InherenceSetting AddInherenceSetting(string name, long accountId);

		bool RemoveInherenceSetting(string name);

		#endregion Inherence

		#region External IdPs

		ExternalIdentityProvider GetExternalIdentityProvider(string name);
		ExternalIdentityProvider AddExternalIdentityProvider(string name, string alias, string description, long accountId);

		#endregion External IdPs

		#region Election Committee
		long AddPoll(string pollName, long accountId);
		void SetPollState(long pollId, int state);
		EcPollRecord GetEcPoll(long pollId, bool includeCandidates = false, bool includeSelections = false);
		List<EcPollRecord> GetEcPolls(int pollState);
		List<EcPollRecord> GetEcPolls();
		long AddCandidateToPoll(long pollId, string candidateName, string assetId);
		void SetCandidateStatus(long candidateId, bool isActive);
		EcCandidateRecord GetCandidateRecord(long candidateId);

		EcPollSelection AddPollSelection(long pollId, string ecCommitment, string ecBlindingFactor);
		EcPollSelection GetPollSelection(long pollId, string ecCommitment);
		EcPollSelection UpdatePollSelection(long pollId, string ecCommitment, string voterBlindingFactor);
		#endregion Election Committee
	}
}
