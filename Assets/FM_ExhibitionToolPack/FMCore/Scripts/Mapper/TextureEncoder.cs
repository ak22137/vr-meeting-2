using System.Collections;
using UnityEngine;
using System;

public class TextureEncoder : MonoBehaviour
{
    public Texture2D StreamTexture;

    public bool FastMode = false;
    public bool AsyncMode = false;
    public bool GZipMode = false;

    [Range(10, 100)]
    public int Quality = 40;

    [Range(0f, 60f)]
    public float StreamFPS = 20f;
    private float interval = 0.05f;

    public bool ignoreSimilarTexture = true;
    private int lastRawDataByte = 0;
    [Tooltip("Compare previous image data size(byte)")]
    public int similarByteSizeThreshold = 8;

    private bool NeedUpdateTexture = false;
    private bool EncodingTexture = false;

    public UnityEventByteArray OnDataByteReadyEvent = new UnityEventByteArray();

    //[Header("Pair Encoder & Decoder")]
    public int label = 1001;
    private int dataID = 0;
    private int maxID = 1024;
    private int chunkSize = 8096; //8096 //32768 //1400
    private float next = 0f;
    private bool stop = false;
    private byte[] dataByte;

    public int dataLength;

    //texture settings
    private byte[] RawTextureData;
    private int width = 8;
    private int height = 8;

    private void Start()
    {
        Application.runInBackground = true;
        StartCoroutine(SenderCOR());
    }

    public void Action_StreamTexture(byte[] inputRawTextureData, int inputWidth, int inputHeight)
    {
        int stride = inputRawTextureData.Length / (inputWidth * inputHeight);
        if (stride != 3 && stride != 4)
        {
            Debug.LogError("unsupported stride count: " + stride + ", only RGB24(stride: 3) and RGBA32(stride: 4) are supported");
            return;
        }

        RawTextureData = new byte[inputRawTextureData.Length];
        Buffer.BlockCopy(inputRawTextureData, 0, RawTextureData, 0, inputRawTextureData.Length);

        width = inputWidth;
        height = inputHeight;
        NeedUpdateTexture = true;
    }

    public void Action_StreamTexture(Texture2D inputTexture2D)
    {
        if (inputTexture2D == null) return;
        if (inputTexture2D.format != TextureFormat.RGB24 && inputTexture2D.format != TextureFormat.RGBA32)
        {
            Debug.LogError("unsupported formmat: " + inputTexture2D.format + ", only RGB24 and RGBA32 are supported");
            return;
        }
        Action_StreamTexture(inputTexture2D.GetRawTextureData(), inputTexture2D.width, inputTexture2D.height);
    }

    void RequestTextureUpdate()
    {
        if (StreamTexture != null) Action_StreamTexture(StreamTexture);

        if (!NeedUpdateTexture) return;
        if (!EncodingTexture)
        {
            //update it now
            EncodingTexture = true;
            StartCoroutine(EncodeBytes());

            NeedUpdateTexture = false;
        }
    }

    IEnumerator SenderCOR()
    {
        while (!stop)
        {
            if (Time.realtimeSinceStartup > next)
            {
                if (StreamFPS > 0)
                {
                    interval = 1f / StreamFPS;
                    next = Time.realtimeSinceStartup + interval;

                    RequestTextureUpdate();
                }

                yield return null;
            }
            yield return null;
        }
    }

    IEnumerator EncodeBytes()
    {
        //==================getting byte data==================
#if UNITY_IOS && !UNITY_EDITOR
            FastMode = true;
#endif

#if UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_EDITOR_OSX || UNITY_EDITOR_WIN || UNITY_IOS || UNITY_ANDROID || WINDOWS_UWP
        if (FastMode)
        {
            //try AsyncMode, on supported platform
            if (AsyncMode && Loom.numThreads < Loom.maxThreads)
            {
                //has spare thread
                bool AsyncEncoding = true;
                Loom.RunAsync(() => {
                    dataByte = RawTextureData.FMRawTextureDataToJPG(width, height, Quality);
                    AsyncEncoding = false;
                });
                while (AsyncEncoding) yield return null;
            }
            else
            {
                //need yield return, in order to fix random error "coroutine->IsInList()"
                yield return dataByte = RawTextureData.FMRawTextureDataToJPG(width, height, Quality);
            }
        }
        else
        {
            dataByte = RawTextureData.FMRawTextureDataToJPG(width, height, Quality);
        }
#else
            dataByte = RawTextureData.FMRawTextureDataToJPG(width, height, Quality);
#endif

        if (ignoreSimilarTexture)
        {
            float diff = Mathf.Abs(lastRawDataByte - dataByte.Length);
            if (diff < similarByteSizeThreshold)
            {
                EncodingTexture = false;
                yield break;
            }
        }
        lastRawDataByte = dataByte.Length;

        if (GZipMode) dataByte = dataByte.FMZipBytes();

        dataLength = dataByte.Length;
        //==================getting byte data==================
        int _length = dataByte.Length;
        int _offset = 0;

        byte[] _meta_label = BitConverter.GetBytes(label);
        byte[] _meta_id = BitConverter.GetBytes(dataID);
        byte[] _meta_length = BitConverter.GetBytes(_length);

        int chunks = Mathf.FloorToInt(dataByte.Length / chunkSize);
        for (int i = 0; i <= chunks; i++)
        {
            int SendByteLength = (i == chunks) ? (_length % chunkSize + 17) : (chunkSize + 17);
            byte[] _meta_offset = BitConverter.GetBytes(_offset);
            byte[] SendByte = new byte[SendByteLength];

            Buffer.BlockCopy(_meta_label, 0, SendByte, 0, 4);
            Buffer.BlockCopy(_meta_id, 0, SendByte, 4, 4);
            Buffer.BlockCopy(_meta_length, 0, SendByte, 8, 4);

            Buffer.BlockCopy(_meta_offset, 0, SendByte, 12, 4);
            SendByte[16] = (byte)(GZipMode ? 1 : 0);

            Buffer.BlockCopy(dataByte, _offset, SendByte, 17, SendByte.Length - 17);
            OnDataByteReadyEvent.Invoke(SendByte);
            _offset += chunkSize;
        }

        dataID++;
        if (dataID > maxID) dataID = 0;

        EncodingTexture = false;
        yield break;
    }

    void OnEnable() { StartAll(); }
    void OnDisable() { StopAll(); }
    void OnApplicationQuit() { StopAll(); }
    void OnDestroy() { StopAll(); }

    void StopAll()
    {
        stop = true;
        StopAllCoroutines();
    }

    void StartAll()
    {
        if (Time.realtimeSinceStartup < 3f) return;
        stop = false;
        StartCoroutine(SenderCOR());

        NeedUpdateTexture = false;
        EncodingTexture = false;
    }
}
