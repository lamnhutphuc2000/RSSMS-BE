using Firebase.Auth;
using Firebase.Storage;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RSSMS.DataService.Services
{
    public interface IFirebaseService
    {
        Task<string> UploadImageToFirebase(string image, string type, int id, string name);
    }
    public class FirebaseService : IFirebaseService
    {
        private static string apiKEY = "AIzaSyCbxMnxwCfJgCJtvaBeRdvvZ3y1Ucuyv2s";
        private static string Bucket = "rssms-5fcc8.appspot.com";
        public FirebaseService()
        {

        }

        public async Task<string> UploadImageToFirebase(string image, string type, int id, string name)
        {
            if (image == null) return null;
            if (image.Length <= 0) return null;

            byte[] data = System.Convert.FromBase64String(image);
            MemoryStream ms = new MemoryStream(data);

            var auth = new FirebaseAuthProvider(new FirebaseConfig(apiKEY));
            var a = await auth.SignInWithEmailAndPasswordAsync("toadmin@gmail.com", "123456");

            var cancellation = new CancellationTokenSource();

            var upload = new FirebaseStorage(
                        Bucket,
                        new FirebaseStorageOptions
                        {
                            AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                            ThrowOnCancel = true
                        }).Child("assets")
                        .Child($"{type}")
                        .Child($"{id}")
                        .Child($"{name}.jpg")
                        .PutAsync(ms, cancellation.Token);
            string url = await upload;

            return url;
        }
    }
}
