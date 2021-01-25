using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using O10.Core;
using O10.Core.Architecture;

using O10.Core.Configuration;
using O10.Core.Logging;
using O10.IdentityProvider.DataLayer.Configuration;
using O10.IdentityProvider.DataLayer.Model;
using O10.IdentityProvider.DataLayer.Model.Enums;

namespace O10.IdentityProvider.DataLayer.Services
{
	[RegisterDefaultImplementation(typeof(IDataAccessService), Lifetime = LifetimeManagement.Singleton)]
	public class DataAccessService : IDataAccessService
	{
		private readonly object _sync = new object();
		private DataContext _dataContext;
		private readonly IEnumerable<IDataContext> _dataContexts;
		private readonly IDataContextConfiguration _configuration;
		private readonly ILogger _logger;

		public DataAccessService(IEnumerable<IDataContext> dataContexts, IConfigurationService configurationService, ILoggerService loggerService)
		{
			_dataContexts = dataContexts;
			_configuration = configurationService.Get<IDataContextConfiguration>();
			_logger = loggerService.GetLogger(nameof(DataAccessService));
		}

		public void Initialize()
		{
			lock (_sync)
			{
				_dataContext = _dataContexts.FirstOrDefault(d => d.DataProvider.Equals(_configuration.ConnectionType)) as DataContext;
				_dataContext.Initialize(_configuration.ConnectionString);
				_dataContext.Database.Migrate();
				_logger.Info($"ConnectionString = {_dataContext.Database.GetDbConnection().ConnectionString}");
			}
		}

		public long AddAssetRegistrationSession(string sessionKey, string sessionCommitment)
        {
			lock(_sync)
			{
				RegistrationSession registrationSession = new RegistrationSession
				{
					CreationTime = DateTime.UtcNow,
					SessionKey = sessionKey,
					SessionCommitment = sessionCommitment
				};

				_dataContext.RegistrationSessions.Add(registrationSession);

				_dataContext.SaveChanges();

				return registrationSession.RegistrationSessionId;
			}
		}

        public long AddAssetRegistration(string assetId, string issuanceCommitment, string biometricCommitment, string protectionCommitment, long issuanceBlindingRecordId)
        {
            lock(_sync)
            {
                UserRecord userRecord = new UserRecord
                {
                    AssetId = assetId,
                    IssuanceCommitment = issuanceCommitment,
					IssuanceBiometricCommitment = biometricCommitment,
					ProtectionCommitment = protectionCommitment,
                    IssuanceBlindingRecordId = issuanceBlindingRecordId,
                    CreationTime = DateTime.UtcNow,
                    LastUpdateTime = DateTime.UtcNow,
                    Status = UserRecordStatus.New
                };

                _dataContext.UserRecords.Add(userRecord);

                _dataContext.SaveChanges();

                return userRecord.UserRecordId;
            }
        }

		public UserRecord GetAssetRegistration(string assetId)
		{
			lock(_sync)
			{
				return _dataContext.UserRecords.FirstOrDefault(r => r.AssetId == assetId);
			}
		}

		public UserRecord RemoveAssetRegistration(string assetId)
		{
			lock (_sync)
			{
				UserRecord userRecord = _dataContext.UserRecords.FirstOrDefault(r => r.AssetId == assetId);

				if(userRecord != null)
				{
					_dataContext.UserRecords.Remove(userRecord);
					_dataContext.SaveChanges();
				}

				return userRecord;
			}
		}

		public long AddIssuanceBlindingFactors(string issuanceBlindingFactor, string biometricBlindingFactor)
		{
			lock(_sync)
			{
				BlindingFactorsRecord blindingFactorsRecord = new BlindingFactorsRecord
				{
					IssuanceBlindingFactor = issuanceBlindingFactor,
                    BiometricBlindingFactor = biometricBlindingFactor
				};

				_dataContext.BlindingFactorsRecords.Add(blindingFactorsRecord);

				_dataContext.SaveChanges();

				return blindingFactorsRecord.BlindingFactorsRecordId;
			}
		}

		public BlindingFactorsRecord GetBlindingFactors(long blindingFactorsId)
		{
			lock(_sync)
			{
				return _dataContext.BlindingFactorsRecords.FirstOrDefault(r => r.BlindingFactorsRecordId == blindingFactorsId);
			}
		}

		public bool DoesAssetExist(string assetId)
		{
			lock(_sync)
			{
				return _dataContext.UserRecords.Any(r => r.AssetId == assetId);
			}
		}

		public RegistrationSession GetAssetRegistrationSession(string sessionKey, string sessionCommitment)
		{
			lock(_sync)
			{
				RegistrationSession registrationSession = 
                    _dataContext.RegistrationSessions.Local.FirstOrDefault(s => s.SessionKey == sessionKey && s.SessionCommitment == sessionCommitment) ??
                    _dataContext.RegistrationSessions.FirstOrDefault(s => s.SessionKey == sessionKey && s.SessionCommitment == sessionCommitment);

                return registrationSession;
			}
		}

        public void RemoveAssetRegistrationSession(long registrationSessionId)
        {
            lock(_sync)
            {
                RegistrationSession registrationSession = 
                    _dataContext.RegistrationSessions.Local.FirstOrDefault(r => r.RegistrationSessionId == registrationSessionId) ?? 
                    _dataContext.RegistrationSessions.FirstOrDefault(r => r.RegistrationSessionId == registrationSessionId);

                if(registrationSession != null)
                {
                    _dataContext.RegistrationSessions.Remove(registrationSession);
                }

                _dataContext.SaveChanges();
            }
        }


        public bool UpdateRegistrationWithBiomteric(string issuanceCommitment, string biometricCommitment, string biometricBlindingFactor)
		{
			lock(_sync)
			{
				UserRecord userRecord = _dataContext.UserRecords.FirstOrDefault(u => u.IssuanceCommitment == issuanceCommitment);

				if(userRecord != null)
				{
					BlindingFactorsRecord blindingFactorsRecord = _dataContext.BlindingFactorsRecords.FirstOrDefault(b => b.BlindingFactorsRecordId == userRecord.IssuanceBlindingRecordId);

					userRecord.IssuanceBiometricCommitment = biometricCommitment;
					blindingFactorsRecord.BiometricBlindingFactor = biometricBlindingFactor;

					_dataContext.SaveChanges();
				}

				return false;
			}
		}

		public void SetIdentityProviderAccountId(long accountId)
		{
			lock(_sync)
			{
				IdentityProviderSettings identityProviderSettings = _dataContext.IdentityProviderSettings.FirstOrDefault();

				if(identityProviderSettings == null)
				{
					identityProviderSettings = new IdentityProviderSettings
					{
						AccountId = accountId
					};

					_dataContext.IdentityProviderSettings.Add(identityProviderSettings);
				}
				else
				{
					identityProviderSettings.AccountId = accountId;
				}

				_dataContext.SaveChanges();
			}
		}

		public void ClearIdentityProviderAccountId()
		{
			lock(_sync)
			{
				IdentityProviderSettings identityProviderSettings = _dataContext.IdentityProviderSettings.FirstOrDefault();
				if(identityProviderSettings != null)
				{
					identityProviderSettings.AccountId = 0;
				}

				_dataContext.SaveChanges();
			}
		}

		public long GetIdentityProviderAccountId()
		{
			lock (_sync)
			{
				IdentityProviderSettings identityProviderSettings = _dataContext.IdentityProviderSettings.FirstOrDefault();
				if (identityProviderSettings != null)
				{
					return identityProviderSettings.AccountId;
				}

				return 0;
			}
		}

        public long AddBiometricRecord(long userRecordId, BiometricRecordType recordType, byte[] content)
        {
            lock(_sync)
            {
                BiometricRecord biometricRecord = new BiometricRecord
                {
                    UserRecordId = userRecordId,
                    RecordType = recordType,
                    Content = content
                };

                _dataContext.BiometricRecords.Add(biometricRecord);

                _dataContext.SaveChanges();

                return biometricRecord.BiometricRecordId;
            }
        }

        public BiometricRecord GetBiometricRecord(long userRecordId, BiometricRecordType recordType)
        {
            lock(_sync)
            {
                return _dataContext.BiometricRecords.FirstOrDefault(r => r.UserRecordId == userRecordId && r.RecordType == recordType);
            }
        }
    }
}
