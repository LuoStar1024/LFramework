using Obfuz;
using Obfuz.EncryptionVM;
using UnityEngine;

namespace Launcher
{
    public static class ObfuzInitialize
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void SetUpStaticSecretKey()
        {
#if ENABLE_OBFUZ
            TextAsset secretKey = Resources.Load<TextAsset>("Obfuz/defaultStaticSecretKey");
            if (secretKey == null)
            {
                Debug.LogError("Obfuz static secret key missing: Resources/Obfuz/defaultStaticSecretKey.bytes");
                return;
            }

            EncryptionService<DefaultStaticEncryptionScope>.Encryptor =
                new GeneratedEncryptionVirtualMachine(secretKey.bytes);
#endif
        }
    }
}
