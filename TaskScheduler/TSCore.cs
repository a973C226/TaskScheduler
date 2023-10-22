using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using TaskScheduler.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Principal;
using System.Configuration;
using System.Net.Mail;
using System.Net;

namespace TaskScheduler
{
    public static class TSCore
    {
        private static ApplicationContext? DB;

        private static TokenValidationParameters GetValidationParameters()
        {
            return new TokenValidationParameters()
            {
                ValidateLifetime = false, 
                ValidateAudience = false, 
                ValidateIssuer = false, 
                ValidIssuer = AuthOptions.ISSUER,
                ValidAudience = AuthOptions.AUDIENCE,
                IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey() 
            };
        }

        public static async Task SendEmail(List<string> assigmentsTitles, string token)
        {
            string email = ConfigurationManager.AppSettings["email"];
            string password = ConfigurationManager.AppSettings["password"];
            string emailTo = GetUserFromToken(token).Username;
            MailAddress from = new MailAddress(email);
            MailAddress to = new MailAddress(emailTo);
            MailMessage m = new MailMessage(from, to);
            m.Subject = "Напоминание";
            m.Body = "Дедлайн в ближайший час:\n" + string.Join('\n', assigmentsTitles);
            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
            smtp.Credentials = new NetworkCredential(email, password);
            smtp.EnableSsl = true;
            try
            {
                await smtp.SendMailAsync(m);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка отправки напоминания: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string GetPasswordBytesString(SecureString ss1)
        {
            IntPtr bstr1 = IntPtr.Zero;
            string passwordBytesString = "";

            try
            {
                bstr1 = Marshal.SecureStringToBSTR(ss1);
                int length = Marshal.ReadInt32(bstr1, -4);
                for (int x = 0; x < length; ++x)
                {
                    byte b1 = Marshal.ReadByte(bstr1, x);
                    if (b1.ToString() != "0")
                        passwordBytesString += b1.ToString();
                }
                return passwordBytesString;
            }
            finally
            {
                if (bstr1 != IntPtr.Zero) Marshal.ZeroFreeBSTR(bstr1);
            }
        }

        private static User GetUserFromToken(string token) 
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(token);

            string username = jwtSecurityToken.Claims.First(claim => claim.Type == "username").Value;
            User user = DB.Users.FirstOrDefault(u => u.Username == username);
            return user;
        }

        public static bool ValidateToken(string authToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = GetValidationParameters();

            SecurityToken validatedToken;
            try
            {
                IPrincipal principal = tokenHandler.ValidateToken(authToken, validationParameters, out validatedToken);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Невалидный JWT токен! Пройдите аутентификацию.", "Невалидный JWT токен", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            
        }

        public static bool IsEqualTo(SecureString password, SecureString passwordRepeat)
        {
            if (GetPasswordBytesString(password) == GetPasswordBytesString(passwordRepeat))
                return true;
            return false;
        }

        public static bool CreateContext()
        {
            try
            {
                DB = new ApplicationContext();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при подключении к БД: " + ex.Message, "Подключение к БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return false;
        }

        public static string AuthUser(string login, SecureString password)
        {
            string encryptedPassword = GetPasswordBytesString(password);
            User user = new User { Username = login, Password = encryptedPassword };
            var userExist = DB.Users.FirstOrDefault(u => u.Username == login && u.Password == encryptedPassword);

            if (userExist != null)
            {
                var claims = new List<Claim> { new Claim("username", login) };
                // создаем JWT-токен
                var jwt = new JwtSecurityToken(
                        issuer: AuthOptions.ISSUER,
                        audience: AuthOptions.AUDIENCE,
                        claims: claims,
                        expires: DateTime.UtcNow.AddDays(1),
                        signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
                
                return new JwtSecurityTokenHandler().WriteToken(jwt);
            }
            else
            {
                MessageBox.Show("Неверные логин или пароль!", "Авторизация", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return "";
        }

        public static bool RegisterUser(string login, SecureString password)
        {
            string encryptedPassword = GetPasswordBytesString(password);
            User user = new User { Username = login, Password = encryptedPassword };

            try
            {
                DB.Users.Add(user);
                DB.SaveChanges();
                return true;
            }
            catch (DbUpdateException ex)
            {
                DB.Users.Remove(user);
                MessageBox.Show("Ошибка: пользователь с таким Email уже существует...", "Регистрация", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Регистрация", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public static Assignment? AddAssignment(string token, string name, string deadline, string description, string date)
        {
            User user = GetUserFromToken(token);

            Assignment assignment = new Assignment { Name = name, Description = description, Date = date, Deadline = deadline, User = user };

            try
            {
                DB.Assignments.Add(assignment);
                DB.SaveChanges();
                return DB.Assignments.ToList().Last();
            }
            catch (Exception ex)
            {
                DB.Assignments.Remove(assignment);
                MessageBox.Show("Ошибка: " + ex.InnerException, "Регистрация", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public static List<Assignment> GetAssignments(string token, string date)
        {
            User user = GetUserFromToken(token);

            List<Assignment> assignments = DB.Assignments.Where(a => a.UserId == user.Id && a.Date == date).OrderBy(a => !a.IsActive).ThenBy(a => a.Deadline).ThenBy(a => a.Name).ToList();
            return assignments;
        }

        public static bool EditAssignment(int id, Dictionary<string, object> fieldsAndValues)
        {
            Assignment? assignment = DB.Assignments.Find(id);
            if (assignment != null) 
            {
                foreach (var field in fieldsAndValues.Keys)
                {
                    if (field.ToLower() == "name")
                    {
                        assignment.Name = (string)fieldsAndValues[field];
                    }
                    if (field.ToLower() == "description")
                    {
                        assignment.Description = (string)fieldsAndValues[field];
                    }
                    if (field.ToLower() == "deadline")
                    {
                        assignment.Deadline = (string)fieldsAndValues[field];
                    }
                    if (field.ToLower() == "isactive")
                    {
                        assignment.IsActive = (bool)fieldsAndValues[field];
                    }
                    if (field.ToLower() == "ismuted")
                    {
                        assignment.IsMuted = (bool)fieldsAndValues[field];
                    }
                }
                try
                {
                    DB.SaveChanges();
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.InnerException, "Редактирование задачи", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            return false;
        }

        public static bool DeleteAssignment(int id)
        {
            Assignment? assignment = DB.Assignments.Find(id);
            if ( assignment != null ) 
            {
                DB.Assignments.Remove(assignment);
                try
                {
                    DB.SaveChanges();
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.InnerException, "Удаление задачи", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            return false;
        }
    }
}
