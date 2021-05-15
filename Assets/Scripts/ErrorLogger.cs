using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using UnityEngine;

public class ErrorLogger : MonoBehaviour {
    [SerializeField] bool isLogging; 
    [SerializeField] bool isSending; 
    
    const string LogPath = "Logs";
    const string Format = "yyyy-MM-dd HH-mm-ss";
    
    DateTime _enableTime;
    readonly List<string> _logNames = new List<string>();
    bool _enabled;
    bool Enabled {
        set {
            if (!isLogging) {
                return;
            }

            if (_enabled == value) {
                return;
            }

            if (value) {
                Application.logMessageReceived += LogCallback;
                _logNames.Clear();
                _enableTime = DateTime.Now;
                _enableTime = _enableTime.AddMilliseconds(-_enableTime.Millisecond);
            } else {
                Application.logMessageReceived -= LogCallback;
            }
            
            _enabled = value;
        }
    }
    
    void OnEnable() {
        Enabled = true;
    }

    void OnDisable() {
        Enabled = false;
    }
    
    void LogCallback(string condition, string stackTrace, LogType type) {
        if (type != LogType.Exception) {
            return;
        }
        
        Directory.CreateDirectory(LogPath);

        var datetime = DateTime.Now.ToString(Format);
        var filename = $"{datetime}-exception.log";
        var filepath = Path.Combine(LogPath, filename);
        _logNames.Add(filepath);
        using (var sw = File.AppendText(filepath)) {
            sw.WriteLine(condition);
            sw.WriteLine(stackTrace);
            sw.WriteLine();
        }
        
        Enabled = false;
        if (isSending) {
            SendMail(datetime);
        }
        
        Quitter.Quit();
    }

    void SendMail(string crashTime) {
        const string from = "cybernotifier@gmail.com";
        const string password = "";
        const string to = "namedengine@gmail.com";
        
        var smtpClient = new SmtpClient("smtp.gmail.com") {
            Port = 587,
            Credentials = new NetworkCredential(from, password),
            EnableSsl = true,
        };
        
        var mailMessage = new MailMessage {
            From = new MailAddress(from),
            To = { to },
            Subject = $"Crash-{crashTime}",
            Body = "",
        };
        _logNames.ForEach(f => mailMessage.Attachments.Add(new Attachment(f, MediaTypeNames.Text.Plain)));
        
        smtpClient.Send(mailMessage);
    }
}
