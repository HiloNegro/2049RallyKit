using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SendEmailButton : ActionEvent
{
    public string Subject;
    [TextArea]
    public string Message;

    public InputField ToEmail;
    public InputField ToName;

    public Camera RenderTextureCamera;

    public GameObject LoadingPanel;
    public TextMeshProUGUI LoadingMessage;
    public TweenAnchorPosition MailPanelTweenPosition;
    public GameObject ShareButtonGameObject;
    //public MagazineLayoutManager MagazineManager;

    private string _magazineCoverFileName = "Acertijo - Portada de revista.png";

    public override void InstantTriggerAction()
    {
        base.InstantTriggerAction();

        LoadingMessage.text = "Enviando correo...";

        LoadingPanel.SetActive(true);
    }

    public override void DelayedTriggerAction()
    {
        base.DelayedTriggerAction();

        SaveTextureToFile();

        SendMail();
    }

    private void SaveTextureToFile()
    {
        FileStream fs = null;
        try
        {
            // Remember currently active render texture
            RenderTexture currentActiveRT = RenderTexture.active;

            RenderTexture.active = RenderTextureCamera.targetTexture;
            Texture2D newTexture = new Texture2D(RenderTextureCamera.targetTexture.width, RenderTextureCamera.targetTexture.height, TextureFormat.ARGB32, false);
            newTexture.ReadPixels(new Rect(0, 0, RenderTextureCamera.targetTexture.width, RenderTextureCamera.targetTexture.height), 0, 0, false);
            newTexture.Apply();

            string persistentDataPath = Application.persistentDataPath;
#if UNITY_ANDROID
            persistentDataPath = AndroidPersistentDataPath.persistentDataPath;
#endif

            byte[] bytes = newTexture.EncodeToPNG();
            fs = new FileStream(persistentDataPath + @"\" + _magazineCoverFileName, FileMode.CreateNew, FileAccess.ReadWrite);
            fs.Write(bytes, 0, bytes.Length);
            //File.WriteAllBytes(persistentDataPath + @"\" + _magazineCoverFileName, bytes);

            // Restorie previously active render texture
            RenderTexture.active = currentActiveRT;

            Destroy(newTexture);

            LoadingMessage.text = "Imagen guardada correctamente...";
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            LoadingMessage.text = "Error al guardar imagen...";
            //LoadingMessage.text = e.Message;
        }

        if (fs != null)
            fs.Close();
    }

    private void SendMail()
    {
        string persistentDataPath = Application.persistentDataPath;
#if UNITY_ANDROID
        persistentDataPath = AndroidPersistentDataPath.persistentDataPath;
#endif

        // Re-open the file at the beginning to make the attachment.
        FileStream fs = new FileStream(persistentDataPath + @"\" + _magazineCoverFileName, FileMode.Open, FileAccess.Read);

        //For File Attachment, more files can also be attached
        ContentType ct = new ContentType();
        ct.MediaType = "image/png";
        ct.Name = _magazineCoverFileName;

        Attachment att = new Attachment(fs, ct);

        string sender = "acertijo.portadaenigma@gmail.com";

        SmtpClient client = new SmtpClient();
        client.Host = "smtp.gmail.com";
        client.Port = 587;
        client.DeliveryMethod = SmtpDeliveryMethod.Network;
        client.UseDefaultCredentials = false;
        System.Net.NetworkCredential credentials = new System.Net.NetworkCredential(sender, "En1gma17");
        client.EnableSsl = true;
        client.Credentials = (System.Net.ICredentialsByHost)credentials;

        LoadingMessage.text = "Enviando correo...";

        try
        {
            var mail = new MailMessage(sender, ToEmail.text.Trim());
            mail.Subject = Subject;
            mail.Body = Message.Replace("{0}", ToName.text.Trim());
            mail.Attachments.Add(att);
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
            {
                return true;
            };

            client.Send(mail);

            LoadingMessage.text = "Correo enviado";
            //MagazineManager.SoftResetCover();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            //LoadingMessage.text = ex.Message;
            LoadingMessage.text = "Error al enviar correo...";
        }

        fs.Close();
        StartCoroutine(HideLoadingPanel(2.0f));
    }

    public void ResetData()
    {
        ToEmail.DeactivateInputField();
        ToName.DeactivateInputField();

        LoadingPanel.SetActive(false);
        ShareButtonGameObject.SetActive(true);

        ToEmail.text = string.Empty;
        ToName.text = string.Empty;

        string persistentDataPath = Application.persistentDataPath;
#if UNITY_ANDROID
        persistentDataPath = AndroidPersistentDataPath.persistentDataPath;
#endif

        if (IsFileReady(persistentDataPath + @"\" + _magazineCoverFileName))
            File.Delete(persistentDataPath + @"\" + _magazineCoverFileName);
    }

    private IEnumerator HideLoadingPanel(float time)
    {
        ToEmail.DeactivateInputField();
        ToName.DeactivateInputField();

        //LoadingMessage.text = "Hiding...";
        yield return new WaitForSeconds(time);

        MailPanelTweenPosition.StartTween(false);

        yield return new WaitForSeconds(/*MailPanelTweenPosition.AnimationTime*/ 0.5f);

        LoadingPanel.SetActive(false);
        ShareButtonGameObject.SetActive(true);

        ToEmail.text = string.Empty;
        ToName.text = string.Empty;

        string persistentDataPath = Application.persistentDataPath;
#if UNITY_ANDROID
        persistentDataPath = AndroidPersistentDataPath.persistentDataPath;
#endif

        yield return new WaitForSeconds(1.0f);

        if (IsFileReady(persistentDataPath + @"\" + _magazineCoverFileName))
            File.Delete(persistentDataPath + @"\" + _magazineCoverFileName);
    }

    private bool IsFileReady(String sFilename)
    {
        // If the file can be opened for exclusive access it means that the file
        // is no longer locked by another process.
        try
        {
            using (FileStream inputStream = File.Open(sFilename, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                if (inputStream.Length > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }
        catch (Exception)
        {
            return false;
        }
    }
}