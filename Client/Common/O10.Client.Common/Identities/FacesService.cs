using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.Services;
using O10.Core;
using O10.Core.Architecture;

using O10.Core.Logging;
using O10.Crypto.ConfidentialAssets;

namespace O10.Client.Common.Identities
{
    [RegisterDefaultImplementation(typeof(IFacesService), Lifetime = LifetimeManagement.Singleton)]
    public class FacesService : IFacesService, IDisposable
    {
        private const string _subscriptionKey = "63274d5208f6421b85d84a1949def81b";
        private const string _faceEndpoint = "https://westus.api.cognitive.microsoft.com";

        private readonly IFaceClient _faceClient = new FaceClient(new ApiKeyServiceClientCredentials(_subscriptionKey));
        private readonly ILogger _logger;
        private readonly IDataAccessService _externalDataAccessService;
        private readonly object _sync = new object();
        private bool _isInitialized;
        private byte[] _expandedSecretKey;
        private byte[] _publicKey;

        private readonly ConcurrentDictionary<string, PersonGroup> _personGroups = new ConcurrentDictionary<string, PersonGroup>();
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, Person>> _personsByGroup = new ConcurrentDictionary<string, ConcurrentDictionary<string, Person>>();

        public FacesService(ILoggerService loggerService, IDataAccessService externalDataAccessService)
        {
            _logger = loggerService.GetLogger(nameof(FacesService));
            _externalDataAccessService = externalDataAccessService;
        }

        public bool Initialize()
        {
            if (_isInitialized)
            {
                return false;
            }

            lock (_sync)
            {
                if (_isInitialized)
                {
                    return false;
                }

                _isInitialized = true;
            }

            try
            {
                if (Uri.IsWellFormedUriString(_faceEndpoint, UriKind.Absolute))
                {
                    _faceClient.Endpoint = _faceEndpoint;

                    var personGroups = _faceClient.PersonGroup.ListAsync().Result;
                    foreach (var item in personGroups)
                    {
                        _personGroups.AddOrUpdate(item.PersonGroupId, item, (k, v) => item);
                    }

                    foreach (var group in _personGroups.Values)
                    {
                        ConcurrentDictionary<string, Person> people = _personsByGroup.GetOrAdd(group.PersonGroupId, new ConcurrentDictionary<string, Person>());
                        IList<Person> peopleSource = _faceClient.PersonGroupPerson.ListAsync(group.PersonGroupId).Result;

                        foreach (var person in peopleSource)
                        {
                            string key = person.UserData ?? person.Name;
                            if (!string.IsNullOrEmpty(key))
                            {
                                people.AddOrUpdate(person.UserData ?? person.Name, person, (g, p) => p);
                            }
                        }
                    }
                }
                else
                {
                    _logger.Error($"Invalid URI {_faceEndpoint}");
                    //TODO: throw Exception
                }

                byte[] secretKey = _externalDataAccessService.GetBiometricSecretKey();
                _expandedSecretKey = CryptoHelper.GetExpandedPrivateKey(secretKey);
                _publicKey = CryptoHelper.GetPublicKeyFromSeed(secretKey);
            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            {
                return false;
            }

            return true;
        }

        private PersonGroup GetOrAddPersonGroup(string personGroupId)
        {
            PersonGroup personGroup = _personGroups.GetOrAdd(personGroupId,
                                                             key => AsyncUtil.RunSync(async () => await CreatePersonGroup(key).ConfigureAwait(false)));

            return personGroup;
        }

        private async Task<PersonGroup> CreatePersonGroup(string personGroupId)
        {
            PersonGroup personGroup = null;
            bool proceed = false;

            try
            {
                await _faceClient.PersonGroup.CreateAsync(personGroupId, personGroupId, personGroupId).ContinueWith(t =>
                    {
                        if (!t.IsFaulted)
                        {
                            proceed = true;
                        }
                    }, TaskScheduler.Default).ConfigureAwait(false);

            }
            catch (AggregateException ex)
            {
                _logger.Error($"Failure during creating Person Group {personGroupId}", ex.InnerException);
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failure during creating Person Group {personGroupId}", ex);
                throw;
            }

            if (proceed)
            {
                proceed = false;

                try
                {
                    await _faceClient.PersonGroup.GetAsync(personGroupId).ContinueWith(t =>
                        {
                            if (t.IsCompleted && !t.IsFaulted)
                            {
                                personGroup = t.Result;
                                proceed = true;
                            }
                        }, TaskScheduler.Default).ConfigureAwait(false);

                }
                catch (AggregateException ex)
                {
                    _logger.Error($"Failure during getting Person Group {personGroupId}", ex.InnerException);
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failure during getting Person Group {personGroupId}", ex);
                    throw;
                }
            }

            return personGroup;
        }

        public async Task<Guid> AddPerson(PersonFaceData facesData)
        {
            _logger.LogIfDebug(() => $"{nameof(AddPerson)}, {nameof(facesData.PersonGroupId)}={facesData.PersonGroupId}, {nameof(facesData.UserData)}={facesData.UserData}, {nameof(facesData.Name)}={facesData.Name}, length of {nameof(facesData.ImageContent)}={facesData.ImageContent?.Length ?? -1}");
            PersonGroup personGroup = GetOrAddPersonGroup(facesData.PersonGroupId);

            if (personGroup != null)
            {
                if (!_personsByGroup[personGroup.PersonGroupId].TryGetValue(facesData.UserData, out Person person))
                {
                    _logger.LogIfDebug(() => $"No Person found with {nameof(facesData.UserData)}={facesData.UserData}. New Person will be created.");
                    person = await _faceClient.PersonGroupPerson.CreateAsync(facesData.PersonGroupId, facesData.Name, facesData.UserData).ConfigureAwait(false);
                    string key = person.UserData ?? facesData.UserData;
                    _logger.LogIfDebug(() => $"Created person with {nameof(facesData.UserData)}={key} and {nameof(person.PersonId)}={person.PersonId}");
                    _personsByGroup[personGroup.PersonGroupId].AddOrUpdate(key, person, (g, p) => p);
                }

                if ((person.PersistedFaceIds?.Count ?? 0) == 0)
                {
                    _logger.LogIfDebug(() => $"No faces detected for person with {nameof(person.PersonId)}={person.PersonId}. New face adding.");
                    using MemoryStream ms = new MemoryStream(facesData.ImageContent);

                    PersistedFace persistedFace = await _faceClient.PersonGroupPerson.AddFaceFromStreamAsync(facesData.PersonGroupId, person.PersonId, ms, facesData.UserData).ConfigureAwait(false);
                }
                else
                {
                    _logger.LogIfDebug(() => $"There are faces detected for person with {nameof(person.PersonId)}={person.PersonId}. No new faces will be added.");
                }

                return person.PersonId;
            }

            _logger.Error($"No Person Group found with {nameof(facesData.PersonGroupId)}={facesData.PersonGroupId}");

            return Guid.Empty;
        }

        public async Task<bool> RemovePerson(PersonFaceData facesData)
        {
            if (_personGroups.TryGetValue(facesData.PersonGroupId, out PersonGroup personGroup))
            {
                if (_personsByGroup.TryGetValue(personGroup.PersonGroupId, out ConcurrentDictionary<string, Person> dict))
                {
                    if (dict.TryRemove(facesData.UserData, out Person person))
                    {
                        try
                        {
                            foreach (var faceId in person.PersistedFaceIds)
                            {
                                await _faceClient.PersonGroupPerson.DeleteFaceAsync(personGroup.PersonGroupId, person.PersonId, faceId).ConfigureAwait(false);
                            }

                            await _faceClient.PersonGroupPerson.DeleteAsync(personGroup.PersonGroupId, person.PersonId).ConfigureAwait(false);
                            return true;
                        }
                        catch (APIErrorException ex)
                        {
                            _logger.Error($"Failed to remove person with personGroupId={facesData.PersonGroupId} and userData={facesData.UserData} due to API Error {ex.Body.Error.Code} '{ex.Body.Error.Message}'", ex);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error($"Failed to remove person with personGroupId={facesData.PersonGroupId} and userData={facesData.UserData}", ex);
                        }
                    }
                }
            }

            return false;
        }

        public async Task<Guid> ReplacePersonFace(PersonFaceData facesData)
        {
            _logger.LogIfDebug(() => $"{nameof(ReplacePersonFace)}, {nameof(facesData.PersonGroupId)}={facesData.PersonGroupId}, {nameof(facesData.UserData)}={facesData.UserData}, {nameof(facesData.Name)}={facesData.Name}, length of {nameof(facesData.ImageContent)}={facesData.ImageContent?.Length ?? -1}");
            PersonGroup personGroup = GetOrAddPersonGroup(facesData.PersonGroupId);

            if (personGroup != null)
            {
                if (_personsByGroup[personGroup.PersonGroupId].TryGetValue(facesData.UserData, out Person person))
                {
                    _logger.LogIfDebug(() => $"{nameof(ReplacePersonFace)}, Person found for {nameof(facesData.PersonGroupId)}={facesData.PersonGroupId} and {nameof(facesData.UserData)}={facesData.UserData}, {nameof(person.PersonId)}={person.PersonId}");
                    if (person.PersistedFaceIds?.Count > 0)
                    {
                        foreach (var faceId in person.PersistedFaceIds)
                        {
                            _logger.LogIfDebug(() => $"{nameof(ReplacePersonFace)}, removing persisted face {faceId} of the person {person.PersonId}");
                            await _faceClient.PersonGroupPerson.DeleteFaceAsync(personGroup.PersonGroupId, person.PersonId, faceId).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        _logger.LogIfDebug(() => $"{nameof(ReplacePersonFace)}, no persisted faces found for person {person.PersonId}");
                    }

                    _logger.LogIfDebug(() => $"{nameof(ReplacePersonFace)}, adding persisted face for person {person.PersonId}");
                    using MemoryStream ms = new MemoryStream(facesData.ImageContent);

                    PersistedFace persistedFace = await _faceClient.PersonGroupPerson.AddFaceFromStreamAsync(facesData.PersonGroupId, person.PersonId, ms, facesData.UserData).ConfigureAwait(false);
                    _logger.LogIfDebug(() => $"{nameof(ReplacePersonFace)}, persisted face for person {person.PersonId} with {nameof(persistedFace.PersistedFaceId)}={persistedFace.PersistedFaceId} added");

                    Person personNew = await _faceClient.PersonGroupPerson.GetAsync(personGroup.PersonGroupId, person.PersonId).ConfigureAwait(false);

                    _personsByGroup[personGroup.PersonGroupId].AddOrUpdate(person.UserData, personNew, (_, __) => personNew);

                    return person.PersonId;
                }
                else
                {
                    _logger.Error($"{nameof(ReplacePersonFace)}, no Person found for {nameof(facesData.PersonGroupId)}={facesData.PersonGroupId} and {nameof(facesData.UserData)}={facesData.UserData}");
                }
            }
            else
            {
                _logger.Error($"No Person Group found with {nameof(facesData.PersonGroupId)}={facesData.PersonGroupId}");
            }


            return Guid.Empty;
        }

        public async Task<Tuple<bool, double>> VerifyPerson(string personGroupId, Guid personId, string imagePath)
        {
            Tuple<bool, double> res = new Tuple<bool, double>(false, 0);

            using (Stream imageFileStream = File.OpenRead(imagePath))
            {
                try
                {
                    IList<DetectedFace> detectedFaces = await _faceClient.Face.DetectWithStreamAsync(imageFileStream).ConfigureAwait(false);

                    if (detectedFaces.Count > 0)
                    {
                        VerifyResult verifyResult = await _faceClient.Face.VerifyFaceToPersonAsync(detectedFaces[0].FaceId.Value, personId, personGroupId).ConfigureAwait(false);
                        res = new Tuple<bool, double>(verifyResult.IsIdentical, verifyResult.Confidence);
                    }

                }
                catch (Exception ex)
                {

                }
            }

            return res;
        }

        public async Task<(bool isIdentical, double confidence)> VerifyPerson(string personGroupId, Guid personId, byte[] imageBytes)
        {
            _logger.Debug($"{nameof(VerifyPerson)}, {nameof(personGroupId)}={personGroupId}, {nameof(personId)}={personId}, length of {nameof(imageBytes)}={imageBytes?.Length ?? -1}");
            (bool isIdentical, double confidence) res = (false, 0);

            try
            {
                using MemoryStream imageStream = new MemoryStream(imageBytes);

                IList<DetectedFace> detectedFaces = await _faceClient.Face.DetectWithStreamAsync(imageStream).ConfigureAwait(false);

                if (detectedFaces.Count > 0)
                {
                    VerifyResult verifyResult = await _faceClient.Face.VerifyFaceToPersonAsync(detectedFaces[0].FaceId.Value, personId, personGroupId).ConfigureAwait(false);
                    res = (verifyResult.IsIdentical, verifyResult.Confidence);
                }
            }
            catch (APIErrorException ex)
            {
                _logger.Error($"Failed to verify face of person due to API Error {ex.Body.Error.Code} '{ex.Body.Error.Message}'", ex);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to verify face of person", ex);
            }

            return res;
        }

        public async Task<bool> VerifyFaces(byte[] face1, byte[] face2)
        {
            using (MemoryStream ms1 = new MemoryStream(face1))
            using (MemoryStream ms2 = new MemoryStream(face2))
            {
                IList<DetectedFace> detectedFaces1 = await _faceClient.Face.DetectWithStreamAsync(ms1).ConfigureAwait(false);
                Guid detectedFaceGuid1 = detectedFaces1.Single(f => f.FaceId.HasValue).FaceId.Value;

                IList<DetectedFace> detectedFaces2 = await _faceClient.Face.DetectWithStreamAsync(ms2).ConfigureAwait(false);
                Guid detectedFaceGuid2 = detectedFaces2.Single(f => f.FaceId.HasValue).FaceId.Value;

                VerifyResult verifyResult = await _faceClient.Face.VerifyFaceToFaceAsync(detectedFaceGuid1, detectedFaceGuid2).ConfigureAwait(false);

                return verifyResult.IsIdentical;
            }
        }

        public void AddPersonGroup(string groupId, string name, string userData)
        {
            _faceClient.PersonGroup.CreateAsync(groupId, name, userData).Wait();
        }

        public IList<DetectedFace> DetectedFaces(string imagePath)
        {
            // The list of Face attributes to return.
            IList<FaceAttributeType> faceAttributes =
                new FaceAttributeType[]
                {
                    FaceAttributeType.Gender, FaceAttributeType.Age,
                    FaceAttributeType.Smile, FaceAttributeType.Emotion,
                    FaceAttributeType.Glasses, FaceAttributeType.FacialHair
                };

            // Call the Face API.
            try
            {
                using (Stream imageFileStream = File.OpenRead(imagePath))
                {
                    // The second argument specifies to return the faceId, while
                    // the third argument specifies not to return face landmarks.
                    IList<DetectedFace> faceList = _faceClient.Face.DetectWithStreamAsync(imageFileStream, true, false, faceAttributes).Result;
                    return faceList;
                }
            }
            catch (Exception e)
            {
                _logger.Error("Error while detecting faces", e);
                return new List<DetectedFace>();
            }
        }

        public async Task<IList<PersonGroup>> GetPersonGroups()
        {
            IList<PersonGroup> personGroups = await _faceClient.PersonGroup.ListAsync().ConfigureAwait(false);

            return personGroups;
        }

        public async Task<TrainingStatus> GetPersonGroupTrainingStatus(string personGroupId)
        {
            return await _faceClient.PersonGroup.GetTrainingStatusAsync(personGroupId).ConfigureAwait(false);
        }

        public Task<IList<Person>> GetPersons(string personGroupId)
        {
            return _faceClient.PersonGroupPerson.ListAsync(personGroupId);
        }

        public async Task StartPersonGroupTraining(string personGroupId)
        {
            await _faceClient.PersonGroup.TrainAsync(personGroupId).ConfigureAwait(false);
        }

        public Tuple<byte[], byte[]> Sign(byte[] msg)
        {
            byte[] signature = CryptoHelper.Sign(msg, _expandedSecretKey);

            return new Tuple<byte[], byte[]>(_publicKey, signature);
        }

        public async Task GetAllPersons(string personGroupId)
        {
            IList<Person> people = await _faceClient.PersonGroupPerson.ListAsync(personGroupId).ConfigureAwait(false);


        }

        #region IDisposable Support
        private bool _disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _faceClient.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~FacesService() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion
    }
}
