using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System;
using System.Threading;
using UnityEngine.Networking;
using OggVorbisEncoder;

using Newtonsoft.Json.Linq;

public class RARE : MonoBehaviour {

	private int headerSize = 44; //default for uncompressed wav
	private bool recOutput;
	//public reference to volume slider
	private Queue<float> dataQ = new Queue<float>();
	private float outputVol = 0.65f;
	private bool micActive = false;
	private bool audioListenerActive = false;
	private GameObject microphoneGO;
	private float[] audioLevels = new float[2048];
	private AudioSource recSource;
	private bool micPlayBack = false;

    private Thread saveFileToDiskThread = null;
    private Thread encodeFileToOggThread = null;
    private Coroutine oggRecording = null;
    //private bool oggRecordingNow = false;

	public static RARE _Instance;

	public static RARE Instance {
		get {
			return _Instance;
		}
	}

	private void Awake(){
		if (_Instance == null) {
			_Instance = this;
		}

		//uncomment this line if you want RARE to be active accross multiple scenes, or you can just add RARE as a component to each scene you use
		//DontDestroyOnLoad (this.gameObject); 

		//prepares microphone game object
		microphoneGO = new GameObject();
		microphoneGO.name = "Microphone";
		recSource = microphoneGO.AddComponent<AudioSource>();
		recSource.loop = true;
	}

	//*********************************************************************************************
	//  MICROPHONE RECORDING FUNCTIONS START >>>>>>>> MICROPHONE RECORDING FUNCTIONS START>>>>>>>
	//*********************************************************************************************

	public void StartMicRecording(int maxLengthInSeconds = 300){
		if (!micActive) {

            if (oggRecording != null)
            {
                //oggRecordingNow = false;
            }

            micActive = true;
			AudioSource aud = microphoneGO.GetComponent<AudioSource> ();
			int minFreq, maxFreq;
			Microphone.GetDeviceCaps (Microphone.devices[0], out minFreq, out maxFreq);
			if (maxFreq > 0) {
				aud.clip = Microphone.Start (Microphone.devices[0], false, maxLengthInSeconds, maxFreq);
			} else {
				aud.clip = Microphone.Start (Microphone.devices[0], false, maxLengthInSeconds, AudioSettings.outputSampleRate);
			}
            if (micPlayBack) {
				while (!(Microphone.GetPosition (null) > 0)) {}
				recSource.Play ();
			}
		} else {
			Debug.Log ("Cannot record multiple things at once, stop current recording to try again");
		}
	}

    //public bool StopMicRecording(string fileName, Action<AudioClip, string> callBackFunction = null, GameObject popUp = null)
    //{
    //    if (micActive)
    //    {
    //        if (fileName.Length > 0)
    //        {
    //            //STOP RECORDING
    //            micActive = false;
    //            if (popUp != null)
    //            {
    //                popUp.SetActive(true);
    //            }
    //            int timeSinceStart = Microphone.GetPosition("");
    //            if (timeSinceStart == 0)
    //            {
    //                Debug.Log("Recording length = 0? -> not a long enough recording to process");
    //                return false;
    //            }
    //            AudioSource aud = microphoneGO.GetComponent<AudioSource>();
    //            Microphone.End("");
    //            float[] recordedClip = new float[aud.clip.samples * aud.clip.channels];
    //            aud.clip.GetData(recordedClip, 0);

    //            float[] doubleClip = new float[recordedClip.Length * 2];
    //            for (int i = 0; i < recordedClip.Length; i++)
    //            {
    //                doubleClip[i * 2] = recordedClip[i];
    //                doubleClip[i * 2 + 1] = recordedClip[i];
    //            }
    //            //shortenedClip is the recorded audio with the extra silence trimmed off
    //            float[] shortenedClip = new float[timeSinceStart * 2];
    //            Array.Copy(doubleClip, shortenedClip, shortenedClip.Length - 1);
    //            //put audio in dataQ so ListenerRecordStop() can use it
    //            FileStream fileStream;
    //            fileStream = new FileStream(Application.persistentDataPath + "/" + fileName + ".wav", FileMode.Create);
    //            byte emptyByte = new byte();
    //            for (int i = 0; i < headerSize; i++) //preparing the header 
    //            {
    //                fileStream.WriteByte(emptyByte);
    //            }
    //            int outputRate = aud.clip.frequency;
    //            for (int i = 0; i < shortenedClip.Length; i++)
    //            {
    //                float temp = shortenedClip[i] * outputRate * outputVol;
    //                if (temp >= Int16.MinValue && temp <= Int16.MaxValue)
    //                {
    //                    byte[] temp2 = BitConverter.GetBytes(Convert.ToInt16(temp));
    //                    fileStream.Write(temp2, 0, temp2.Length);
    //                }
    //            }

    //            FinishWritingFile(fileStream, outputRate, 2);

    //            if (popUp != null)
    //            {
    //                popUp.SetActive(false);
    //            }

    //            if (callBackFunction != null)
    //            {
    //                StartCoroutine(LoadClip(fileName, callBackFunction));
    //            }

    //            return true;
    //        }
    //        else
    //        {
    //            Debug.Log("StopMicRecording requires fileName length greater than 0");
    //            return false;
    //        }
    //    }
    //    else
    //    {
    //        Debug.Log("Microphone recording cannot be stopped if it hasn't started yet.");
    //        return false;
    //    }
    //}

    public bool StopMicRecording(string fileName, Action<AudioClip, string> callBackFunction = null, GameObject popUp = null)
    {
        if (!micActive)
        {
            Debug.Log("Microphone recording cannot be stopped if it hasn't started yet.");
            return false;
        }

        if (fileName.Length <= 0)
        {
            Debug.Log("StopMicRecording requires fileName length greater than 0");
            return false;
        }

        micActive = false;

        int timeSinceStart = Microphone.GetPosition("");
        if (timeSinceStart == 0)
        {
            Debug.Log("Recording length = 0? -> not a long enough recording to process");
            return false;
        }

        StartCoroutine(StopMicRecording(fileName, timeSinceStart, callBackFunction));

        return true;
    }

    private IEnumerator StopMicRecording(string fileName, int time, Action<AudioClip, string> callbackFunction)
    {
        AudioSource aud = microphoneGO.GetComponent<AudioSource>();
        Microphone.End("");
        float[] recordedClip = new float[aud.clip.samples * aud.clip.channels];
        aud.clip.GetData(recordedClip, 0);

        int frequency = aud.clip.frequency;
        int samples = aud.clip.samples;
        string wavPath = Application.persistentDataPath + "/" + fileName + ".wav";

        Debug.Log("Save to disk thread started");

        saveFileToDiskThread = new Thread(() => SaveFileToDisk(wavPath, recordedClip, time, frequency));
        saveFileToDiskThread.Start();

        while (saveFileToDiskThread.IsAlive)
        {
            yield return null;
        }

        Debug.Log("Save to disk thread finished");

        //Debug.Log("Encode to Ogg thread started");

        //encodeFileToOggThread = new Thread(() => EncodeFileToOgg(wavPath, oggPath, frequency, samples));
        //encodeFileToOggThread.Start();

        //while(encodeFileToOggThread.IsAlive)
        //{
        //    yield return null;
        //}

        //Debug.Log("Encode to Ogg Thread finished");

        if (callbackFunction != null)
        {
            StartCoroutine(LoadClip(fileName, callbackFunction));
        }
    }

    private void SaveFileToDisk(string path, float[] recordedClip, int time, int frequency)
    {
        float[] doubleClip = new float[recordedClip.Length * 2];
        for (int i = 0; i < recordedClip.Length; i++)
        {
            doubleClip[i * 2] = recordedClip[i];
            doubleClip[i * 2 + 1] = recordedClip[i];
        }
        
        float[] shortenedClip = new float[time * 2];
        Array.Copy(doubleClip, shortenedClip, shortenedClip.Length);
        
        FileStream fileStream = new FileStream(path, FileMode.Create);
        byte emptyByte = new byte();
        for (int i = 0; i < headerSize; i++) //preparing the header 
        {
            fileStream.WriteByte(emptyByte);
        }
        int outputRate = frequency;
        for (int i = 0; i < shortenedClip.Length; i++)
        {
            float temp = shortenedClip[i] * outputRate * outputVol;
            if (temp >= Int16.MinValue && temp <= Int16.MaxValue)
            {
                byte[] temp2 = BitConverter.GetBytes(Convert.ToInt16(temp));
                fileStream.Write(temp2, 0, temp2.Length);
            }
        }

        FinishWritingFile(fileStream, outputRate, 2);
    }

    public void EncodeAndSendEmail(string fileName, string wavPath, string oggPath, int frequency, int samples, ActivityProgress progress, Action callbackFunction)
    {
        StartCoroutine(EncodeAndSendEmailCoroutine(fileName, wavPath, oggPath, frequency, samples, progress, callbackFunction));
    }

    private IEnumerator EncodeAndSendEmailCoroutine(string fileName, string wavPath, string oggPath, int frequency, int samples, ActivityProgress progress, Action callbackFunction)
    {
        while (encodeFileToOggThread != null)
        {
            yield return null;
        }

        Debug.Log("Encode to Ogg thread started");

        encodeFileToOggThread = new Thread(() => EncodeFileToOgg(wavPath, oggPath, frequency, samples));
        encodeFileToOggThread.Start();

        while (encodeFileToOggThread.IsAlive)
        {
            yield return null;
        }

        Debug.Log("Encode to Ogg Thread finished");

        // Re-open the file at the beginning to make the attachment.
        FileStream fs = new FileStream(oggPath, FileMode.Open, FileAccess.Read);

        //For File Attachment, more files can also be attached
        ContentType ct = new ContentType();
        ct.MediaType = "audio/ogg";
        ct.Name = fileName;

        Attachment att = new Attachment(fs, ct);

        string sender = "audios2049rally@gmail.com";

        SmtpClient client = new SmtpClient();
        client.Host = "smtp.gmail.com";
        client.Port = 587;
        client.DeliveryMethod = SmtpDeliveryMethod.Network;
        client.UseDefaultCredentials = false;
        System.Net.NetworkCredential credentials = new System.Net.NetworkCredential(sender, "En1gma17");
        client.EnableSsl = true;
        client.Credentials = (System.Net.ICredentialsByHost)credentials;
        
        //LoadingMessage.text = "Enviando correo...";

        try
        {
            var mail = new MailMessage(sender, "fabrica.hilonegro@gmail.com");
            mail.Subject = "Audio 2049";
            mail.Body = @"¡Hola! Han recibido un nuevo audio de la aplicación 2049.";
            mail.Attachments.Add(att);
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
            {
                return true;
            };

            client.Send(mail);

            //LoadingMessage.text = "Correo enviado";
            //MagazineManager.SoftResetCover();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            //LoadingMessage.text = ex.Message;
            //LoadingMessage.text = "Error al enviar correo...";
        }

        fs.Close();

        encodeFileToOggThread = null;

        if (callbackFunction != null)
        {
            callbackFunction();
        }
    }

    public void EncodeAndUpload(string fileName, string wavPath, string oggPath, int frequency, int samples, ActivityProgress progress, Action callbackFunction)
    {
        StartCoroutine(EncodeAndUploadCoroutine(fileName, wavPath, oggPath, frequency, samples, progress, callbackFunction));
    }

    private IEnumerator EncodeAndUploadCoroutine(string fileName, string wavPath, string oggPath, int frequency, int samples, ActivityProgress progress, Action callbackFunction)
    {
        while (encodeFileToOggThread != null)
        {
            yield return null;
        }

        Debug.Log("Encode to Ogg thread started");

        encodeFileToOggThread = new Thread(() => EncodeFileToOgg(wavPath, oggPath, frequency, samples));
        encodeFileToOggThread.Start();

        while (encodeFileToOggThread.IsAlive)
        {
            yield return null;
        }

        Debug.Log("Encode to Ogg Thread finished");


        Debug.Log("Uploading Ogg started");
        FileStream fileStream = new FileStream(oggPath, FileMode.Open, FileAccess.Read);
        byte[] contents = new byte[fileStream.Length];
        fileStream.Read(contents, 0, contents.Length);

        WWWForm form = new WWWForm();
        form.AddBinaryData("file", contents, fileName + ".ogg", "audio/ogg");
        form.AddField("fileslot", fileName);

        using (UnityWebRequest www = UnityWebRequest.Post("http://enjambre.cultura.gob.mx/api/retos/upload/" + ProgressTracker.Instance.ProgressState.entryId + "/" + ProgressTracker.Instance.ProgressState.sessionToken, form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
                fileStream.Close();
                ProgressTracker.Instance.Logout();
                yield break;
            }
            else
            {
                JObject entry = JObject.Parse(www.downloadHandler.text);
                if (!entry["mensaje"].Value<string>().Equals("ok"))
                {
                    Debug.Log("Error");
                    fileStream.Close();
                    ProgressTracker.Instance.Logout();
                    yield break;
                }
                else
                {
                    Debug.Log(www.downloadHandler.text);
                    progress.audioUrl = entry["filename"].Value<string>();
                    ProgressTracker.Instance.SaveProgress();
                }
            }
        }

        fileStream.Close();
        Debug.Log("Uploading Ogg finished");

        encodeFileToOggThread = null;

        if (callbackFunction != null)
        {
            callbackFunction();
        }
    }

    private void EncodeFileToOgg(string wavPath, string oggPath, int frequency, int samples)
    {
        FileStream fileStream = new FileStream(wavPath, FileMode.Open, FileAccess.Read);
        FileStream stdout = new FileStream(oggPath, FileMode.Create, FileAccess.Write);
        
        var info = VorbisInfo.InitVariableBitRate(2, frequency, 0.2f);
        var serial = new System.Random().Next();
        var oggStream = new OggStream(serial);
        var headerBuilder = new HeaderPacketBuilder();

        var comments = new Comments();
        comments.AddTag("ARTIST", "TEST");

        var infoPacket = headerBuilder.BuildInfoPacket(info);
        var commentsPacket = headerBuilder.BuildCommentsPacket(comments);
        var booksPacket = headerBuilder.BuildBooksPacket(info);

        oggStream.PacketIn(infoPacket);
        oggStream.PacketIn(commentsPacket);
        oggStream.PacketIn(booksPacket);

        // Flush to force audio data onto its own page per the spec
        OggPage page;
        while (oggStream.PageOut(out page, true))
        {
            stdout.Write(page.Header, 0, page.Header.Length);
            stdout.Write(page.Body, 0, page.Body.Length);
        }

        var processingState = ProcessingState.Create(info);

        var buffer = new float[info.Channels][];
        buffer[0] = new float[samples];
        buffer[1] = new float[samples];

        var readbuffer = new byte[samples * 4];
        while (!oggStream.Finished)
        {
            var bytes = fileStream.Read(readbuffer, 0, readbuffer.Length);

            if (bytes == 0)
            {
                processingState.WriteEndOfStream();
            }
            else
            {
                var samplesS = bytes / 4;

                for (var i = 0; i < samplesS; i++)
                {
                    // uninterleave samples
                    buffer[0][i] = (short)((readbuffer[i * 4 + 1] << 8) | (0x00ff & readbuffer[i * 4])) / 32768f;
                    buffer[1][i] = (short)((readbuffer[i * 4 + 3] << 8) | (0x00ff & readbuffer[i * 4 + 2])) / 32768f;
                }

                processingState.WriteData(buffer, samplesS);
            }

            OggPacket packet;
            while (!oggStream.Finished
                   && processingState.PacketOut(out packet))
            {
                oggStream.PacketIn(packet);

                while (!oggStream.Finished
                       && oggStream.PageOut(out page, false))
                {
                    stdout.Write(page.Header, 0, page.Header.Length);
                    stdout.Write(page.Body, 0, page.Body.Length);
                }
            }
        }

        fileStream.Close();
        stdout.Close();
    }

    //public void ConvertMicRecordingToOgg(string fileName, int frequency, int samples, Action<string> callbackFunction)
    //{
    //    oggRecording = StartCoroutine(ConvertToOggCoroutine(fileName, frequency, samples, callbackFunction));
    //}

    //private IEnumerator ConvertToOggCoroutine(string fileName, int frequency, int samples, Action<string> callBackFunction)
    //{
    //    oggRecordingNow = true;

    //    FileStream fileStream = new FileStream(Application.persistentDataPath + "/" + fileName + ".wav", FileMode.Open, FileAccess.Read);
    //    var stdout = new FileStream(Application.persistentDataPath + "/" + fileName + @".ogg", FileMode.Create, FileAccess.Write);
    //    var info = VorbisInfo.InitVariableBitRate(2, frequency, 0.2f);
    //    var serial = new System.Random().Next();
    //    var oggStream = new OggStream(serial);
    //    var headerBuilder = new HeaderPacketBuilder();

    //    var comments = new Comments();
    //    comments.AddTag("ARTIST", "TEST");

    //    var infoPacket = headerBuilder.BuildInfoPacket(info);
    //    var commentsPacket = headerBuilder.BuildCommentsPacket(comments);
    //    var booksPacket = headerBuilder.BuildBooksPacket(info);

    //    oggStream.PacketIn(infoPacket);
    //    oggStream.PacketIn(commentsPacket);
    //    oggStream.PacketIn(booksPacket);

    //    // Flush to force audio data onto its own page per the spec
    //    OggPage page;
    //    while (oggRecordingNow && oggStream.PageOut(out page, true))
    //    {
    //        stdout.Write(page.Header, 0, page.Header.Length);
    //        stdout.Write(page.Body, 0, page.Body.Length);
    //    }

    //    var processingState = ProcessingState.Create(info);

    //    var buffer = new float[info.Channels][];
    //    buffer[0] = new float[samples];
    //    buffer[1] = new float[samples];

    //    var readbuffer = new byte[samples * 4];
    //    while (oggRecordingNow && !oggStream.Finished)
    //    {
    //        var bytes = fileStream.Read(readbuffer, 0, readbuffer.Length);

    //        if (bytes == 0)
    //        {
    //            processingState.WriteEndOfStream();
    //        }
    //        else
    //        {
    //            var samplesS = bytes / 4;

    //            for (var i = 0; i < samplesS; i++)
    //            {
    //                // uninterleave samples
    //                buffer[0][i] = (short)((readbuffer[i * 4 + 1] << 8) | (0x00ff & readbuffer[i * 4])) / 32768f;
    //                buffer[1][i] = (short)((readbuffer[i * 4 + 3] << 8) | (0x00ff & readbuffer[i * 4 + 2])) / 32768f;
    //            }

    //            processingState.WriteData(buffer, samplesS);
    //        }

    //        OggPacket packet;
    //        while (oggRecordingNow && !oggStream.Finished
    //               && processingState.PacketOut(out packet))
    //        {
    //            oggStream.PacketIn(packet);

    //            while (oggRecordingNow && !oggStream.Finished
    //                   && oggStream.PageOut(out page, false))
    //            {
    //                stdout.Write(page.Header, 0, page.Header.Length);
    //                stdout.Write(page.Body, 0, page.Body.Length);
    //            }

    //            yield return null;
    //        }

    //        yield return null;
    //    }

    //    fileStream.Close();
    //    stdout.Close();

    //    if (oggRecordingNow)
    //        yield return StartCoroutine(UploadOgg(fileName, callBackFunction));

    //    oggRecordingNow = false;
    //    oggRecording = null;
    //}

    //private IEnumerator UploadOgg(string fileName, Action<string> callbackFunction)
    //{
    //    FileStream fileStream = new FileStream(Application.persistentDataPath + "/" + fileName + ".ogg", FileMode.Open, FileAccess.Read);

    //    byte[] contents = new byte[fileStream.Length];
    //    fileStream.Read(contents, 0, contents.Length);

    //    WWWForm form = new WWWForm();
    //    form.AddBinaryData("file", contents, fileName + ".ogg", "audio/ogg");
    //    form.AddField("fileslot", fileName);

    //    using (UnityWebRequest www = UnityWebRequest.Post("http://enjambre.cultura.gob.mx/api/retos/upload/" + ProgressTracker.Instance.ProgressState.entryId + "/" + ProgressTracker.Instance.ProgressState.sessionToken, form))
    //    {
    //        yield return www.SendWebRequest();

    //        if (www.isNetworkError || www.isHttpError)
    //        {
    //            Debug.Log(www.error);
    //        }
    //        else
    //        {
    //            JObject entry = JObject.Parse(www.downloadHandler.text);
    //            if (!entry["mensaje"].Value<string>().Equals("ok"))
    //                Debug.Log("Error");
    //            else
    //            {
    //                Debug.Log(www.downloadHandler.text);

    //                if (callbackFunction != null && oggRecordingNow)
    //                {
    //                    callbackFunction(entry["filename"].Value<string>());
    //                }
    //            }
    //        }
    //    }

    //    fileStream.Close();
    //}

    //****************************************************
    //  MICROPHONE RECORDING FUNCTIONS END <<<<<<<<<<<<<<
    //****************************************************

    //*********************************************************************************************
    //  AUDIOLISTENER RECORDING FUNCTIONS START >>>>> AUDIOLISTENER RECORDING FUNCTIONS START >>>>
    //*********************************************************************************************

    public void StartAudioListenerRecording(){
		if (!audioListenerActive) {
			audioListenerActive = true;
			recOutput = true;
		} else {
			Debug.Log ("Cannot record multiple things at once, stop current recording to try again");
		}
	}

	public void StopAudioListenerRecording(string fileName, Action<AudioClip, string> callBackFunction = null, GameObject popUp = null){
		if (audioListenerActive) {
			if (fileName.Length > 0) {
				audioListenerActive = false;
				if (popUp != null) {
					popUp.SetActive (true);
				}
				recOutput = false;
				FileStream fileStream;
				fileStream = new FileStream(Application.persistentDataPath +"/"+ fileName + ".wav", FileMode.Create);
				byte emptyByte = new byte();
				for(int i = 0; i<headerSize; i++) //preparing the header 
				{
					fileStream.WriteByte(emptyByte);
				}
				while(dataQ.Count > 0) {
					float temp = dataQ.Dequeue ();
					if (temp >= Int16.MinValue && temp <= Int16.MaxValue) {
						byte[] temp2 = BitConverter.GetBytes (Convert.ToInt16 (temp));
						fileStream.Write (temp2, 0, temp2.Length);
					}
				}
				FinishWritingFile (fileStream, AudioSettings.outputSampleRate,2);
				dataQ.Clear ();
				if (popUp != null) {
					popUp.SetActive (false);
				}
				if (callBackFunction != null) {
					StartCoroutine (LoadClip (fileName, callBackFunction));
				}
			} else {
				Debug.Log ("StopAudioRecording requires fileName length greater than 0");
			}
		} else {
			Debug.Log ("AudioListener recording cannot be stopped if it hasn't started yet.");
		}
	}

	/*void FixedUpdafte(){
		if (micActive) {
		    recSource.GetSpectrumData (audioLevels, 2, FFTWindow.BlackmanHarris);
		}
	}*/

	public void OnAudioFilterRead(float[] data, int channels){ //this function runs repeatedly during audiolistener recording
		//all the audio data is stuffed into a queue to avoid latency 
		//and then pulled out and processed in writeheader() after the recording has been stopped
		//if (!micActive) {
		audioLevels = data;
		//}
		if(recOutput) { 
			for (int i = 0; i < data.Length; i++) { 			
				dataQ.Enqueue(data [i] * 32767 * outputVol);
			}   
		}
	}

	//****************************************************
	//  AUDIOLISTENER RECORDING FUNCTIONS END <<<<<<<<<<
	//****************************************************

	//*********************************************************************************************
	//  EXPORT FUNCTIONS START >>>>> EXPORT FUNCTIONS START >>>>
	//*********************************************************************************************

	public void ExportClip(string fileName, AudioClip myClip, Action<AudioClip, string> callBackFunction = null, GameObject popUp = null){
		if (popUp != null) {
			popUp.SetActive (true);
		}
		//get audio data stream
		float[] data = new float[myClip.samples*myClip.channels];
		myClip.GetData (data, 0);
		//get correct sample rate-----
		FileStream fileStream;
		fileStream = new FileStream(Application.persistentDataPath +"/"+ fileName + ".wav", FileMode.Create);
		byte emptyByte = new byte();
		for(int i = 0; i<headerSize; i++) { //preparing the header
			fileStream.WriteByte(emptyByte);
		}
		for ( int i = 0; i < data.Length ; i++) {
			float temp = data [i] * 32767;
			if (temp >= Int16.MinValue && temp <= Int16.MaxValue) {
				byte[] temp2 = BitConverter.GetBytes (Convert.ToInt16 (temp));
				fileStream.Write (temp2, 0, temp2.Length);
			}
		}
		FinishWritingFile (fileStream, myClip.frequency, myClip.channels);
		if (popUp != null) {
			popUp.SetActive (false);
		}
		if (callBackFunction != null) {
			StartCoroutine (LoadClip (fileName, callBackFunction));
		}
	}

	//****************************************************
	//  EXPORT FUNCTIONS END <<<<<<<<<<
	//****************************************************

	//*********************************************************************************************
	// GET AUDIOCLIP FUNCTIONS START >>>>> 
	//*********************************************************************************************

	public void GetAudioClipFromFile(string fileName, Action<AudioClip, string> callBackFunction, string fileType = "wav") {
		if(fileType.Equals("wav")||fileType.Equals("mp3")||fileType.Equals("ogg")){
			StartCoroutine (LoadClip (fileName, callBackFunction, fileType));
		}
	}

	IEnumerator LoadClip(string fileName, Action<AudioClip, string> callBackFunction, string fileType = "wav", AudioType audioType = AudioType.WAV) {
        string prefix = "file://";
        if (Application.isEditor)
            prefix += "/";

		using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(prefix + Application.persistentDataPath +"/"+ fileName + "." + fileType, audioType)) {
			yield return www.SendWebRequest();
			if (www.isNetworkError || www.isHttpError) {
                Debug.Log(www.error + " " + www.responseCode);
                callBackFunction(null, null);
			} else {
                Debug.Log("ok");
                callBackFunction(DownloadHandlerAudioClip.GetContent(www), fileName);
			}
		}     
	}

	public void GetAudioClipFromURL(string urlPath, Action<AudioClip, string> callBackFunction){
		StartCoroutine (LoadClipUrl (urlPath, callBackFunction));
	}

	IEnumerator LoadClipUrl(string urlPath, Action<AudioClip, string> callBackFunction){
		WWW www;
		www = new WWW (urlPath);
		yield return www;
		AudioClip myClip = www.GetAudioClip (false, false);
		callBackFunction (myClip, urlPath);
	}

	public AudioClip MakeAudioClipMono(AudioClip myClip) {
		Debug.Log(myClip.channels);
		float[] recordedClip = new float[myClip.samples ];
		myClip.GetData(recordedClip, 0);
		//shortenedClip is the recorded audio with the samples not apart of channels

		List<float> shortenedClip = new List<float>(0);
		// Debug.Log(shortenedClip.Length + "  "  + recordedClip.Length);
		//if clip returns no remainder when divided by the number of channels, which is then asigned to the shorted array to make the clip mono

		for (int i = 0 ; i < recordedClip.Length; i++){
			if (i % myClip.channels == 0) {
				// Debug.Log("auder");\
				shortenedClip.Add ( recordedClip[i]);
			}
		}

		// Array.Copy(recordedClip, shortenedClip, 0, shortenedClip.Length - 1);

		AudioClip newClip = AudioClip.Create("trimmedMonoClip", shortenedClip.ToArray().Length, 1, myClip.frequency, false);
		newClip.SetData(shortenedClip.ToArray(), 0);
		return newClip;
	}

	//****************************************************
	//  GET AUDIOCLIP FUNCTIONS END <<<<<<<<<<
	//****************************************************

	//*********************************************************************************************
	// TRIM AUDIOCLIP FUNCTIONS START >>>>> 
	//*********************************************************************************************

	public void CropAudioClip(string fileName, int frontIndex, int endIndex, AudioClip myClip, Action<AudioClip, string> callBackFunction = null, GameObject popUp = null) {
		myClip = TrimEndOfAudioClip(myClip, endIndex);
		myClip = TrimFrontOfAudioClip(myClip, frontIndex);
		ExportClip (fileName, myClip, callBackFunction, popUp);
	}

	public AudioClip TrimFrontOfAudioClip(AudioClip myClip, int frontIndex) {
		float[] recordedClip = new float[myClip.samples * myClip.channels];
		myClip.GetData(recordedClip, 0);
		//shortenedClip is the recorded audio with the front trimmed off
		float[] shortenedClip = new float[(myClip.samples * myClip.channels) - frontIndex];
		Array.Copy(recordedClip, frontIndex, shortenedClip, 0, shortenedClip.Length - 1);
		AudioClip newClip = AudioClip.Create (myClip.name, shortenedClip.Length / myClip.channels, myClip.channels, myClip.frequency, false);
		newClip.SetData(shortenedClip, 0);
		return newClip;
	}

	public AudioClip TrimEndOfAudioClip(AudioClip myClip, int endIndex) {
		float[] recordedClip = new float[myClip.samples * myClip.channels];
		myClip.GetData(recordedClip, 0);
		//shortenedClip is the recorded audio with the end trimmed off
		float[] shortenedClip = new float[endIndex];
		Array.Copy(recordedClip, shortenedClip, shortenedClip.Length - 1);
		AudioClip newClip = AudioClip.Create (myClip.name, shortenedClip.Length / myClip.channels, myClip.channels, myClip.frequency, false);
		newClip.SetData(shortenedClip, 0);
		return newClip;
	}

	public KeyValuePair<AudioClip, AudioClip> SplitAudioClip(AudioClip myClip, int splitIndex) {
		//AudioClip startClip = TrimEndOfAudioClip(myClip, splitIndex);
		//AudioClip endClip = TrimFrontOfAudioClip(myClip, splitIndex); 
		return new KeyValuePair<AudioClip, AudioClip>(myClip, myClip);
	}

	//****************************************************
	//  TRIM AUDIOCLIP FUNCTIONS END <<<<<<<<<<
	//****************************************************

	//*********************************************************************************************
	// OTHER FUNCTIONS AND HELPER FUNCTIONS START >>>>> 
	//*********************************************************************************************

	private void FinishWritingFile(FileStream fileStream, int outputRate, int channels){
		fileStream.Seek(0,SeekOrigin.Begin);
		byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
		fileStream.Write(riff,0,4);
		byte[]  chunkSize = BitConverter.GetBytes(fileStream.Length-8);
		fileStream.Write(chunkSize,0,4);
		byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
		fileStream.Write(wave,0,4);
		byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
		fileStream.Write(fmt,0,4);
		byte[] subChunk1 = BitConverter.GetBytes(16);
		fileStream.Write(subChunk1,0,4);
		ushort two = 2;
		ushort one = 1;
		byte[] audioFormat = BitConverter.GetBytes(one);
		fileStream.Write(audioFormat,0,2);
		byte[] numChannels = BitConverter.GetBytes(two);
		if (channels == 2)
		{
			numChannels = BitConverter.GetBytes(two);
		} else if (channels == 1)
		{
			numChannels = BitConverter.GetBytes(one);
		} else 
		{//should we try to support 8 channels and change this to a case switch satement? or just support two?
			numChannels = BitConverter.GetBytes(two);
		}

		fileStream.Write(numChannels,0,2);
		byte[] sampleRate = BitConverter.GetBytes(outputRate);
		fileStream.Write(sampleRate,0,4);
		byte[] byteRate = BitConverter.GetBytes(outputRate*4);
		// sampleRate * bytesPerSample*number of channels, here 44100*2*2
		fileStream.Write(byteRate,0,4);
		ushort four = 4;
		byte[] blockAlign = BitConverter.GetBytes(four);
		fileStream.Write(blockAlign,0,2);
		ushort sixteen = 16;
		byte[] bitsPerSample = BitConverter.GetBytes(sixteen);
		fileStream.Write(bitsPerSample,0,2);
		byte[] dataString = System.Text.Encoding.UTF8.GetBytes("data");
		fileStream.Write(dataString,0,4);
		byte[] subChunk2 = BitConverter.GetBytes(fileStream.Length-headerSize);
		fileStream.Write(subChunk2,0,4);
		fileStream.Close();
	}

	public void OutputVolume(float input){
		if (input >= 0.0f && input <= 1.0f) {
			outputVol = input;
		}
	}

	public AudioClip RemoveSilenceFromFrontOfAudioClip(AudioClip myClip) {
		float[] recordedClip = new float[myClip.samples * myClip.channels];
		myClip.GetData(recordedClip, 0);
		int frontIndex = 0;
		bool flagged = false;
		for (int i = 0; i < recordedClip.Length; i++) {
			if (recordedClip [i] > 0f) {
				frontIndex = i;
				flagged = true;
				i = recordedClip.Length;
			}
		}
		if (flagged) {
			return TrimFrontOfAudioClip (myClip, frontIndex);
		} else {
			return myClip;
		}
	}

	public AudioClip RemoveSilenceFromEndOfAudioClip(AudioClip myClip) {
		float[] recordedClip = new float[myClip.samples *myClip.channels];
		myClip.GetData(recordedClip, 0);
		int endIndex = 0;
		bool flagged = false;
		for (int i = recordedClip.Length-1; i >= 0; --i) {
			if (recordedClip[i] > 0f) {
				endIndex = i;
				flagged = true;
				i = -1;
			}
		}
		if (flagged) {
			return TrimEndOfAudioClip (myClip, endIndex);
		} else {
			return myClip;
		}
	}

	public float[] GetAudioLevels(){
		return audioLevels;
	}

	public void SetMicPlayBack(bool trueOrFalse){
		micPlayBack = trueOrFalse;
	}

	public void SetMicPlayBackVolume(float volume){
		if (volume <= 1.0f && volume >= 0.0f) {
			recSource.volume = volume;
		}
	}
	//****************************************************
	//  OTHER FUNCTIONS AND HELPER FUNCTIONS END <<<<<<
	//****************************************************
}

