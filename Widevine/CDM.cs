using AmazonDL.Downloader;
using AmazonDL.UtilLib;
using AmazonDL.UtilLib.Crypto;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace AmazonDL.Widevine
{
    public class CDM
    {
        static Dictionary<string, CDMDevice> Devices { get; } = new Dictionary<string, CDMDevice>()
        {
        };
        static Dictionary<string, Session> Sessions { get; set; } = new Dictionary<string, Session>();

        static CDM()
        {
            Devices.Add("chrome_1610", new CDMDevice("chromecdm_1610",
                Convert.FromBase64String("CAES7AkKrwIIAhIR6i5pjZWyhEFCL0mX1v/VRQoYmI7X5gUijgIwggEKAoIBAQC10dxEGINZbF0nIoMtM8705Nqm6ZWdb72DqTdFJ+UzQIRIUS59lQkYLvdQp71767vz0dVlPTikHmivdYHRc7Fo6JsmSUsGR3th+fU6d1Wt6cwpMTUXj/qODmubDK/ioVDW7wz9OFlSsCBvylOYp9v2+u/VXwACnBXNxCDezjx4RKcqMFT31WTxqU9OM9J86ChMOW4bFA41aLAJozB+02xis7OV175XdQ5vkVXM9ys6ZoRF/K6NXeHiwcZFtMKyphXAxqU7uGY2a16bC3TEG5/km6Jru3Wxy4nKlDyUjWISwH4llWjdSi99r2c1fSCXlMCrW0CHoznn+22lYCKtYe8JAgMBAAEohWsSgAKX5hxfRHB1oHDoOUoq5XNAKzOGHcW0yHkORZtzDuOAOP2dU47IX6kG9XC78HlrIbvdcV05C0lNSXmXnEa0PTvSUkEP+rbH2Fg4d3qK4KnfagZGtt0COrIlr0mIJncyu0DjMMGiZ4dGIE6j/TjeUndpqNKtedlwrmCvxJCjzUpksJYXaK5tAzuK9COeXGrv7s4rRnBYRWBgf9xQBZO2t+akv2lIjAwxE6rQ2BDBPTHu+Pqz+OAskiJo9Gqe+PkL74pCQ04XZfwKiKE/s4bCYM+I/Bp0mTsIgzOKIAJHLxFK+P+dcrmtczlocuu+rWNO6VaOBdS7cqy5gyn0bZ50xetYGrQFCq4CCAESEGB/sJoVhJ/CCR4dlC3oDwEY0cDk5AUijgIwggEKAoIBAQDU0VsrppDpRW0tXycwIP2dYOVUMP4W608/nMIJ1cgZKH1s8V+OhH+XUtCPgeKw+kKGp4egfyURRei0smsSbVrtENnP9XB4XtHOU0mzxEsmMtH1cZEfCQPRIt0FcrYNSjCXjgESOGmQMaF33zLXLT3fuoI46VZ8mk85nMhs80aeRdsfy6rPGMWb2QZ3WuFKiPI/71QiFJmJwfECnOugFi+wFCex0LrK0UTvtsH5WnjI6+Hy5G+o3wTJzP0Cf3jimLe6+OfB53U5J7ypBqRNZo2d5p1S21oT/ojV9aZw48v8uCyFmjc8ytQjWUi6TOiLzZoHMnpvaUzd8gXEB2b5oZbhAgMBAAEohWsSgANdeZYX2x6zEExoCMPP0Jd4+w4Llkeu6eryjS/bt+c+vTvL4Kjcoe22mtn5XIVb9DSiPre7zx78JWz88KKiaws4v0ULOPP4JkQ9exVzyRpjpIpXaJHb6enl6x5CnUBMPu/4QuYvLMbXxvNOXO2s89Er+eSTjgX3Xr9GLJ7BaSJsZTthebt1rLv/5keMe4RQaZJt0jGKUcOTeDCbonEelBM/HwIqLl+hEh8+V/LFdehnb0YAMCShZImMI6AYho+Px8K4auHPHH4nV/CtTLnuGN+tZKapCDuXdZgsoT0dLxigLOb8TJaq540qoAmRCY54RASYqAmcYNLTPLhlDEN6wrmku3ybzszXLDavTPVAuqT78Ge9v3rVLySljoGYk9QkzsTxIrTLg/UZ6fECnUMFkoFvTuCb5KkuQiLVKXYjFcHQHILUQbXBexRZq3N3l88ZqupSOwmLrtMt8ET8O4vrVFL7fgoRbsxIrMFxAunZm9wQC7zS1rfjta4lC1XXHd+ifF8aGwoRYXJjaGl0ZWN0dXJlX25hbWUSBng4Ni02NBoWCgxjb21wYW55X25hbWUSBkdvb2dsZRoXCgptb2RlbF9uYW1lEglDaHJvbWVDRE0aGAoNcGxhdGZvcm1fbmFtZRIHV2luZG93cxojChR3aWRldmluZV9jZG1fdmVyc2lvbhILNC4xMC4xNjc5LjAyCAgAEAAYASAB"),
                Convert.FromBase64String("LS0tLS1CRUdJTiBSU0EgUFJJVkFURSBLRVktLS0tLQpNSUlFb3dJQkFBS0NBUUVBdGRIY1JCaURXV3hkSnlLRExUUE85T1RhcHVtVm5XKzlnNmszUlNmbE0wQ0VTRkV1CmZaVUpHQzczVUtlOWUrdTc4OUhWWlQwNHBCNW9yM1dCMFhPeGFPaWJKa2xMQmtkN1lmbjFPbmRWcmVuTUtURTEKRjQvNmpnNXJtd3l2NHFGUTF1OE0vVGhaVXJBZ2I4cFRtS2ZiOXZydjFWOEFBcHdWemNRZzNzNDhlRVNuS2pCVQo5OVZrOGFsUFRqUFNmT2dvVERsdUd4UU9OV2l3Q2FNd2Z0TnNZck96bGRlK1YzVU9iNUZWelBjck9tYUVSZnl1CmpWM2g0c0hHUmJUQ3NxWVZ3TWFsTzdobU5tdGVtd3QweEJ1ZjVKdWlhN3Qxc2N1SnlwUThsSTFpRXNCK0paVm8KM1VvdmZhOW5OWDBnbDVUQXExdEFoNk01NS90dHBXQWlyV0h2Q1FJREFRQUJBb0lCQUJqand5YnhRaDNlTnp4Ugp2YXBVOHNwVWY5Z2tsczRvQzBYNFJyQXBYM2R1S0Eyc1MxUjJyL21IQ0dVYXFWWks5WDVScGNoSG9yYlkwTlRnCkhhYmFFeG04NmV4S1VVSnBTNnNrYUIwYVUvak1UaDMvZGZpbFJaUG54blJCdnR3ajRDaWtMZCtHTkxnY2t6d3EKY3RvdGRHK3hkMTU2dEVvbkt0Ynh0OXc0V0UvUVB3MFNJMksvR05BRmFMNnRGUTQxcWdOUlZ1VGp2YWU3NU11bgp2and6STBoN25FUHJtaTgxV1VuTzcxam9kTFIrVE5CbFpOZTJLUVlnZjRrNmNPTXdWQ0hHcDNrRjkzbWlIMGdnCnh5dEVnZytpRVJlNUpmcmprYzcxcW9scFJzNlhSdGdmZXJzaitJWGQxcUQzR1o3VE8vVHlwdUhRS0xYWUptcFcKN25oU1g2Y0NnWUVBMU10Qk1nUFJhdEdqeGVDeUF4Nmd6M2JpSnVJUkFHWCtyRUMzZkJYdXNzRE9LZlNqaEZjZQoyRDJoY1VCeFVvQ0lsbDdNSXBYVHlaZkFvT1hETmhNakZOYlhaK2NXa2VGU0E1UEh0aVJBMzRUN1dsckpLU2FkCnExTnNCN3dGem5nQkV1MUJUSzVJU2xhcWxUblc2Q0laUzNYR0tlU3NZaWQ1c0NEVWtqdmVFb2NDZ1lFQTJyeWIKeGZuUEFDRFh3bWpLSEZGOUpKMTgvc1FzSFQrTjlCcUQwQWgydGExSStXMmQ2cDkxek1FbG11ZXllTWQ2VllXSgpvQ2IrSTZSQ3Zpd1ZDeFhzelMxZVRlQXU3eCt0Mm1hT0RoZXlGSG5SUVV1UWVkUE1nS3UwWWo0VGRsVGd2QzBYClE0ZWFLcHhibjQybnhmTnN2WGZ2M013cTFTQnVOdy9panRvK0JlOENnWUE0UHgyVmhkTFdCR0hnelJyZ25qanQKZmNRYmVRZjdiZnBhTjZVSVpKZTZvaUljanZDbDY0MVlwVG5HUUwxempFd09TekowTmR4TVhoTnB0REhjV2tTYwpub2xEaXA2NW9yQldCN3J6VnpQYW9VRDdTaUlBQVpnTmtEaHU5dkVsK1N4M2YwVVNhc0xxKzJ1TmxFTk5DcTVhCjl0K1JkVU5ua24yazQ1aXNxcXh4Z3dLQmdEazYzZHJucUdSVk0zNTNJbUpVWTlTL3ErT1FlaVpRNlpnems4ZEwKWDV2Yk1kdW85WWRjbjFxcU1tZWNOWkxxUmpTNEVyRW5ZcGo2c2tmRml0L3lwWmx0UkY1RnlLSGgvUC9HazJaVwpoczVhclFoVGVBS1lDc3Fqb2plT0hGTjZrNjVJY2V6R1B1emxLZ2ZON1ZhYWdSbjFsbm1EcGJWTG5lcWtLbGZMCi9DeGJBb0dCQUp6cXZpN1BOd21oeUZnbzZWWDRsZ3ZrZTVxbHZxcGRUaHJhR21vN1FPQU00VjgxL0J5RjZXSTcKcVR3WlY1VU5SUlh6M3B1bzhHdFZFMlgvQXFTRy9LVDFDd0RmV1VhOFJ2RmZtNzVHVmxVUkQwMlkrOFR3c0QybQpSelNxQUpvdHcyNyswdVVoR0Eyd1YvemNqd2l4T0xJL3dJRXoyMUxGTFdvc09VNzZ6N0JSCi0tLS0tRU5EIFJTQSBQUklWQVRFIEtFWS0tLS0tCg=="),
                Convert.FromBase64String("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")
            ));
        }

        static byte[] CheckPSSH(string psshB64)
        {
            byte[] systemID = new byte[] { 237, 239, 139, 169, 121, 214, 74, 206, 163, 200, 39, 220, 213, 29, 33, 237 };

            if (psshB64.Length % 4 != 0)
            {
                psshB64 = psshB64.PadRight(psshB64.Length + (4 - (psshB64.Length % 4)), '=');
            }

            byte[] pssh = Convert.FromBase64String(psshB64);

            if (pssh.Length < 30)
                return pssh;

            if (!pssh[12..28].SequenceEqual(systemID))
            {
                List<byte> newPssh = new List<byte>() { 0, 0, 0 };
                newPssh.Add((byte)(32 + pssh.Length));
                newPssh.AddRange(Encoding.UTF8.GetBytes("pssh"));
                newPssh.AddRange(new byte[] { 0, 0, 0, 0 });
                newPssh.AddRange(systemID);
                newPssh.AddRange(new byte[] { 0, 0, 0, 0 });
                newPssh[31] = (byte)(pssh.Length);
                newPssh.AddRange(pssh);

                return newPssh.ToArray();
            }
            else
            {
                return pssh;
            }
        }

        public static string OpenSession(string initDataB64, string deviceName, bool offline = false, bool raw = false)
        {
            byte[] initData = CheckPSSH(initDataB64);

            var device = Devices[deviceName];

            byte[] sessionId = new byte[16];

            if (device.IsAndroid)
            {
                string randHex = "";

                Random rand = new Random();
                string choice = "ABCDEF0123456789";
                for (int i = 0; i < 16; i++)
                    randHex += choice[rand.Next(16)];

                string counter = "01";
                string rest = "00000000000000";
                sessionId = Encoding.ASCII.GetBytes(randHex + counter + rest);
            }
            else
            {
                Random rand = new Random();
                rand.NextBytes(sessionId);
            }

            Session session;
            dynamic parsedInitData = ParseInitData(initData);

            if (parsedInitData != null)
            {
                session = new Session(sessionId, parsedInitData, device, offline);
            }
            else if (raw)
            {
                session = new Session(sessionId, initData, device, offline);
            }
            else
            {
                return null;
            }

            Sessions.Add(Utils.BytesToHex(sessionId), session);

            return Utils.BytesToHex(sessionId);
        }

        static WidevineCencHeader ParseInitData(byte[] initData)
        {
            WidevineCencHeader cencHeader;

            try
            {
                cencHeader = Serializer.Deserialize<WidevineCencHeader>(new MemoryStream(initData[32..]));
            }
            catch
            {
                try
                {
                    //needed for HBO Max

                    PSSHBox psshBox = PSSHBox.FromByteArray(initData);
                    cencHeader = Serializer.Deserialize<WidevineCencHeader>(new MemoryStream(psshBox.Data));
                }
                catch
                {
                    //Logger.Verbose("Unable to parse, unsupported init data format");
                    return null;
                }
            }

            return cencHeader;
        }

        public static bool CloseSession(string sessionId)
        {
            //Logger.Debug($"CloseSession(session_id={Utils.BytesToHex(sessionId)})");
            //Logger.Verbose("Closing CDM session");

            if (Sessions.ContainsKey(sessionId))
            {
                Sessions.Remove(sessionId);
                //Logger.Verbose("CDM session closed");
                return true;
            }
            else
            {
                //Logger.Info($"Session {sessionId} not found");
                return false;
            }
        }

        public static bool SetServiceCertificate(string sessionId, byte[] certData)
        {
            //Logger.Debug($"SetServiceCertificate(sessionId={Utils.BytesToHex(sessionId)}, cert={certB64})");
            //Logger.Verbose($"Setting service certificate");

            if (!Sessions.ContainsKey(sessionId))
            {
                //Logger.Error("Session ID doesn't exist");
                return false;
            }

            SignedMessage signedMessage = new SignedMessage();

            try
            {
                signedMessage = Serializer.Deserialize<SignedMessage>(new MemoryStream(certData));
            }
            catch
            {
                //Logger.Warn("Failed to parse cert as SignedMessage");
            }

            SignedDeviceCertificate serviceCertificate;
            try
            {
                try
                {
                    //Logger.Debug("Service cert provided as signedmessage");
                    serviceCertificate = Serializer.Deserialize<SignedDeviceCertificate>(new MemoryStream(signedMessage.Msg));
                }
                catch
                {
                    //Logger.Debug("Service cert provided as signeddevicecertificate");
                    serviceCertificate = Serializer.Deserialize<SignedDeviceCertificate>(new MemoryStream(certData));
                }
            }
            catch
            {
                //Logger.Error("Failed to parse service certificate");
                return false;
            }

            Sessions[sessionId].ServiceCertificate = serviceCertificate;
            Sessions[sessionId].PrivacyMode = true;

            return true;
        }

        public static byte[] GetLicenseRequest(string sessionId)
        {
            //Logger.Debug($"GetLicenseRequest(sessionId={Utils.BytesToHex(sessionId)})");
            //Logger.Verbose($"Getting license request");

            if (!Sessions.ContainsKey(sessionId))
            {
                //Logger.Error("Session ID doesn't exist");
                return null;
            }

            var session = Sessions[sessionId];

            //Logger.Debug("Building license request");

            dynamic licenseRequest;

            if (session.InitData is WidevineCencHeader)
            {
                licenseRequest = new SignedLicenseRequest
                {
                    Type = SignedLicenseRequest.MessageType.LicenseRequest,
                    Msg = new LicenseRequest
                    {
                        Type = LicenseRequest.RequestType.New,
                        KeyControlNonce = 1093602366,
                        ProtocolVersion = ProtocolVersion.Current,
                        RequestTime = uint.Parse((DateTime.Now - DateTime.UnixEpoch).TotalSeconds.ToString().Split(".")[0]),
                        ContentId = new LicenseRequest.ContentIdentification
                        {
                            CencId = new LicenseRequest.ContentIdentification.Cenc
                            {
                                LicenseType = session.Offline ? LicenseType.Offline : LicenseType.Default,
                                RequestId = session.SessionId,
                                Pssh = session.InitData
                            }
                        }
                    }
                };
            }
            else
            {
                licenseRequest = new SignedLicenseRequestRaw
                {
                    Type = SignedLicenseRequestRaw.MessageType.LicenseRequest,
                    Msg = new LicenseRequestRaw
                    {
                        Type = LicenseRequestRaw.RequestType.New,
                        KeyControlNonce = 1093602366,
                        ProtocolVersion = ProtocolVersion.Current,
                        RequestTime = uint.Parse((DateTime.Now - DateTime.UnixEpoch).TotalSeconds.ToString().Split(".")[0]),
                        ContentId = new LicenseRequestRaw.ContentIdentification
                        {
                            CencId = new LicenseRequestRaw.ContentIdentification.Cenc
                            {
                                LicenseType = session.Offline ? LicenseType.Offline : LicenseType.Default,
                                RequestId = session.SessionId,
                                Pssh = session.InitData
                            }
                        }
                    }
                };
            }

            if (session.PrivacyMode)
            {
                //Logger.Debug("Privacy mode & serivce certificate loaded, encrypting client id");

                EncryptedClientIdentification encryptedClientIdProto = new EncryptedClientIdentification();

                //Logger.Debug("Unencrypted client id " + Utils.SerializeToString(clientId));

                using var memoryStream = new MemoryStream();
                Serializer.Serialize(memoryStream, session.Device.ClientID);
                byte[] data = Padding.AddPKCS7Padding(memoryStream.ToArray(), 16);

                using AesCryptoServiceProvider aesProvider = new AesCryptoServiceProvider
                {
                    BlockSize = 128,
                    Padding = PaddingMode.PKCS7,
                    Mode = CipherMode.CBC
                };
                aesProvider.GenerateKey();
                aesProvider.GenerateIV();

                using MemoryStream mstream = new MemoryStream();
                using CryptoStream cryptoStream = new CryptoStream(mstream, aesProvider.CreateEncryptor(aesProvider.Key, aesProvider.IV), CryptoStreamMode.Write);
                cryptoStream.Write(data, 0, data.Length);
                encryptedClientIdProto.EncryptedClientId = mstream.ToArray();

                using RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
                RSA.ImportRSAPublicKey(session.ServiceCertificate.DeviceCertificate.PublicKey, out int bytesRead);
                encryptedClientIdProto.EncryptedPrivacyKey = RSA.Encrypt(aesProvider.Key, RSAEncryptionPadding.OaepSHA1);
                encryptedClientIdProto.EncryptedClientIdIv = aesProvider.IV;
                encryptedClientIdProto.ServiceId = Encoding.UTF8.GetString(session.ServiceCertificate.DeviceCertificate.ServiceId);
                encryptedClientIdProto.ServiceCertificateSerialNumber = session.ServiceCertificate.DeviceCertificate.SerialNumber;

                licenseRequest.Msg.EncryptedClientId = encryptedClientIdProto;
            }
            else
            {
                licenseRequest.Msg.ClientId = session.Device.ClientID;
            }

            //Logger.Debug("Signing license request");

            using (var memoryStream = new MemoryStream())
            {
                Serializer.Serialize(memoryStream, licenseRequest.Msg);
                byte[] data = memoryStream.ToArray();
                session.LicenseRequest = data;

                licenseRequest.Signature = session.Device.Sign(data);
            }

            //Logger.Verbose("License request created");

            byte[] requestBytes;
            using (var memoryStream = new MemoryStream())
            {
                Serializer.Serialize(memoryStream, licenseRequest);
                requestBytes = memoryStream.ToArray();
            }

            Sessions[sessionId] = session;

            //Logger.Debug($"license request b64: {Convert.ToBase64String(requestBytes)}");
            return requestBytes;
        }

        public static void ProvideLicense(string sessionId, byte[] license)
        {
            //Logger.Debug($"ProvideLicense(sessionId={Utils.BytesToHex(sessionId)}, licenseB64={licenseB64})");
            //Logger.Verbose("Decrypting provided license");

            if (!Sessions.ContainsKey(sessionId))
            {
                throw new Exception("Session ID doesn't exist");
            }

            var session = Sessions[sessionId];

            if (session.LicenseRequest == null)
            {
                throw new Exception("Generate a license request first");
            }

            SignedLicense signedLicense;
            try
            {
                signedLicense = Serializer.Deserialize<SignedLicense>(new MemoryStream(license));
            }
            catch
            {
                throw new Exception("Unable to parse license");
            }

            //Logger.Debug("License: " + Utils.SerializeToString(signedLicense));

            session.License = signedLicense;

            //Logger.Debug($"Deriving keys from session key");

            try
            {
                var sessionKey = session.Device.Decrypt(session.License.SessionKey);

                if (sessionKey.Length != 16)
                {
                    throw new Exception("Unable to decrypt session key");
                }

                session.SessionKey = sessionKey;
            }
            catch
            {
                throw new Exception("Unable to decrypt session key");
            }

            //Logger.Debug("Session key: " + Utils.BytesToHex(session.SessionKey));

            session.DerivedKeys = DeriveKeys(session.LicenseRequest, session.SessionKey);

            //Logger.Debug("Verifying license signature");

            byte[] licenseBytes;
            using (var memoryStream = new MemoryStream())
            {
                Serializer.Serialize(memoryStream, signedLicense.Msg);
                licenseBytes = memoryStream.ToArray();
            }
            byte[] hmacHash = CryptoUtils.GetHMACSHA256Digest(licenseBytes, session.DerivedKeys.Auth1);

            if (!hmacHash.SequenceEqual(signedLicense.Signature))
            {
                throw new Exception("License signature mismatch");
            }

            foreach (License.KeyContainer key in signedLicense.Msg.Keys)
            {
                string type = key.Type.ToString();

                if (type == "Signing")
                    continue;

                byte[] keyId;
                byte[] encryptedKey = key.Key;
                byte[] iv = key.Iv;
                keyId = key.Id;
                if (keyId == null)
                {
                    keyId = Encoding.ASCII.GetBytes(key.Type.ToString());
                }

                byte[] decryptedKey;

                using MemoryStream mstream = new MemoryStream();
                using AesCryptoServiceProvider aesProvider = new AesCryptoServiceProvider
                {
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.PKCS7
                };
                using CryptoStream cryptoStream = new CryptoStream(mstream, aesProvider.CreateDecryptor(session.DerivedKeys.Enc, iv), CryptoStreamMode.Write);
                cryptoStream.Write(encryptedKey, 0, encryptedKey.Length);
                decryptedKey = mstream.ToArray();

                List<string> permissions = new List<string>();
                if (type == "OperatorSession")
                {
                    foreach (PropertyInfo perm in key._OperatorSessionKeyPermissions.GetType().GetProperties())
                    {
                        if ((uint)perm.GetValue(key._OperatorSessionKeyPermissions) == 1)
                        {
                            permissions.Add(perm.Name);
                        }
                    }
                }
                session.ContentKeys.Add(new ContentKey
                {
                    KeyID = keyId,
                    Type = type,
                    Bytes = decryptedKey,
                    Permissions = permissions
                });
            }

            //Logger.Debug($"Key count: {session.Keys.Count}");

            Sessions[sessionId] = session;

            //Logger.Verbose("Decrypted all keys");
        }

        public static DerivedKeys DeriveKeys(byte[] message, byte[] key)
        {
            byte[] encKeyBase = Encoding.UTF8.GetBytes("ENCRYPTION").Concat(new byte[] { 0x0, }).Concat(message).Concat(new byte[] { 0x0, 0x0, 0x0, 0x80 }).ToArray();
            byte[] authKeyBase = Encoding.UTF8.GetBytes("AUTHENTICATION").Concat(new byte[] { 0x0, }).Concat(message).Concat(new byte[] { 0x0, 0x0, 0x2, 0x0 }).ToArray();

            byte[] encKey = new byte[] { 0x01 }.Concat(encKeyBase).ToArray();
            byte[] authKey1 = new byte[] { 0x01 }.Concat(authKeyBase).ToArray();
            byte[] authKey2 = new byte[] { 0x02 }.Concat(authKeyBase).ToArray();
            byte[] authKey3 = new byte[] { 0x03 }.Concat(authKeyBase).ToArray();
            byte[] authKey4 = new byte[] { 0x04 }.Concat(authKeyBase).ToArray();

            byte[] encCmacKey = CryptoUtils.GetCMACDigest(encKey, key);
            byte[] authCmacKey1 = CryptoUtils.GetCMACDigest(authKey1, key);
            byte[] authCmacKey2 = CryptoUtils.GetCMACDigest(authKey2, key);
            byte[] authCmacKey3 = CryptoUtils.GetCMACDigest(authKey3, key);
            byte[] authCmacKey4 = CryptoUtils.GetCMACDigest(authKey4, key);

            byte[] authCmacCombined1 = authCmacKey1.Concat(authCmacKey2).ToArray();
            byte[] authCmacCombined2 = authCmacKey3.Concat(authCmacKey4).ToArray();

            return new DerivedKeys
            {
                Auth1 = authCmacCombined1,
                Auth2 = authCmacCombined2,
                Enc = encCmacKey
            };
        }

        public static List<ContentKey> GetKeys(string sessionId)
        {
            if (Sessions.ContainsKey(sessionId))
                return Sessions[sessionId].ContentKeys;
            else
            {
                throw new Exception("Session not found");
            }
        }
    }
}



/*
        public static List<string> ProvideLicense(string requestB64, string licenseB64)
        {
            byte[] licenseRequest;

            var request = Serializer.Deserialize<SignedLicenseRequest>(new MemoryStream(Convert.FromBase64String(requestB64)));

            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, request.Msg);
                licenseRequest = ms.ToArray();
            }

            SignedLicense signedLicense;
            try
            {
                signedLicense = Serializer.Deserialize<SignedLicense>(new MemoryStream(Convert.FromBase64String(licenseB64)));
            }
            catch
            {
                return null;
            }

            byte[] sessionKey;
            try
            {

                sessionKey = Controllers.Adapter.OaepDecrypt(Convert.ToBase64String(signedLicense.SessionKey));

                if (sessionKey.Length != 16)
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }

            byte[] encKeyBase = Encoding.UTF8.GetBytes("ENCRYPTION").Concat(new byte[] { 0x0, }).Concat(licenseRequest).Concat(new byte[] { 0x0, 0x0, 0x0, 0x80 }).ToArray();

            byte[] encKey = new byte[] { 0x01 }.Concat(encKeyBase).ToArray();

            byte[] encCmacKey = GetCmacDigest(encKey, sessionKey);

            byte[] encryptionKey = encCmacKey;

            List<string> keys = new List<string>();
           
            foreach (License.KeyContainer key in signedLicense.Msg.Keys)
            {
                string type = key.Type.ToString();
                if (type == "Signing")
                {
                    continue;
                }

                byte[] keyId;
                byte[] encryptedKey = key.Key;
                byte[] iv = key.Iv;
                keyId = key.Id;
                if (keyId == null)
                {
                    keyId = Encoding.ASCII.GetBytes(key.Type.ToString());
                }

                byte[] decryptedKey;

                using MemoryStream mstream = new MemoryStream();
                using AesCryptoServiceProvider aesProvider = new AesCryptoServiceProvider
                {
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.PKCS7
                };
                using CryptoStream cryptoStream = new CryptoStream(mstream, aesProvider.CreateDecryptor(encryptionKey, iv), CryptoStreamMode.Write);
                cryptoStream.Write(encryptedKey, 0, encryptedKey.Length);
                decryptedKey = mstream.ToArray();

                List<string> permissions = new List<string>();
                if (type == "OPERATOR_SESSION")
                {
                    foreach (FieldInfo perm in key._OperatorSessionKeyPermissions.GetType().GetFields())
                    {
                        if ((uint)perm.GetValue(key._OperatorSessionKeyPermissions) == 1)
                        {
                            permissions.Add(perm.Name);
                        }
                    }
                }
                keys.Add(BitConverter.ToString(keyId).Replace("-","").ToLower() + ":" + BitConverter.ToString(decryptedKey).Replace("-", "").ToLower());
            }

            return keys;
        }*/
