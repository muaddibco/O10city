using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using O10.Client.Common.Identities;
using O10.Core.Architecture;

namespace O10.Client.Common.Interfaces
{
    [ServiceContract]
    public interface IFacesService
    {
        bool Initialize();

        Task<IList<PersonGroup>> GetPersonGroups();

        void AddPersonGroup(string personGroupId, string name, string userData);

        IList<DetectedFace> DetectedFaces(string imagePath);

        Task<IList<Person>> GetPersons(string personGroupId);

        Task<Guid> AddPerson(PersonFaceData facesData);
        Task<bool> RemovePerson(PersonFaceData facesData);
        Task<Guid> ReplacePersonFace(PersonFaceData facesData);

        Task StartPersonGroupTraining(string personGroupId);

        Task<TrainingStatus> GetPersonGroupTrainingStatus(string personGroupId);

        Task<Tuple<bool, double>> VerifyPerson(string personGroupId, Guid personId, string imagePath);

        Task<(bool isIdentical, double confidence)> VerifyPerson(string personGroupId, Guid personId, byte[] imageBytes);

        Task<bool> VerifyFaces(byte[] face1, byte[] face2);

        Tuple<byte[], byte[]> Sign(byte[] msg);
    }
}
