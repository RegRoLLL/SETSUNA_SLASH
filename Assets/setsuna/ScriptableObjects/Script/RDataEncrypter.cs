using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName ="RDataEncrypter",menuName = "ScriptableObject/RDataEncrypter")]
public class RDataEncrypter : ScriptableObject
{
    [SerializeField] List<long> _keys = new();

    public long EncryptInteger(int data) => EncryptInteger(data, _keys);
    long EncryptInteger(int data, List<long> keys)
    {
        long result = data;

        for(int index = 0; index < keys.Count; index++)
        {
            long key =keys[index];

            if (index == 0)
            {
                result *= key;
            }
            else if (index % 2 == 0)
            {
                result += key;
            }
            else if (index % 2 == 1)
            {
                result -= key;
            }
        }

        return result;
    }

    public int DecryptInteger(long data) => DecryptInteger(data, _keys);
    int DecryptInteger(long data, List<long> keys)
    {

        for (int index = keys.Count-1; index >= 0; index--)
        {
            long key = keys[index];

            if(index == 0)
            {
                data /= key;
            }
            else if (index % 2 == 0)
            {
                data -= key;
            }
            else if (index % 2 == 1)
            {
                data += key;
            }
        }

        return (int)data;
    }

#if UNITY_EDITOR
    [ContextMenu("addKey")]
    void AddKey()
    {
        _keys.Add(Random.Range(100000, 1234567890));
    }

    [ContextMenu("test")]
    void Test()
    {
        for(int i=0; i < 10; i++)
        {
            int data = Random.Range(1,10);
            //List<long> key = new();
            //int keyCount = Random.Range(5, 30);

            //for(int j = 0; j < keyCount; j++)
            //    key.Add(Random.Range(1,1234567890));

            var encryptedData = EncryptInteger(data);
            var decryptedData = DecryptInteger(encryptedData);

            string baseLog = $"origin:{data} | encrypt:{encryptedData} | result:{decryptedData} | ";

            if (decryptedData == data) Debug.Log(baseLog + "correct!!");
            else Debug.LogWarning(baseLog + "incorrect!!");
        }
    }
#endif

}
