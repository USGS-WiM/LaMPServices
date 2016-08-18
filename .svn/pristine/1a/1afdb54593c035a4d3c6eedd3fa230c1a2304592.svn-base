#region Comments
// 02.17.12 - JB - Added Base64 decode method for Basic Auth
// 01.24.12 - JB - Created
#endregion
#region Copywright
/* Authors:
 *      Jonathan Baier (jbaier@usgs.gov)
 * Copyright:
 *      2012 USGS - WiM
 */
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Configuration;
using OpenRasta.Authentication;
using OpenRasta.Authentication.Basic;

namespace LaMPServices.Authentication
{
    public class LaMPBasicAuthentication : IBasicAuthenticator
    {
        public string Realm
        {
            get { return "LaMP-Secured-Domain-Realm"; }
        }


        public AuthenticationResult Authenticate(BasicAuthRequestHeader header)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["LaMPDSEntities"].ConnectionString;
            try
            {
                using (LaMPDSEntities aLDSE = new LaMPDSEntities(string.Format(connectionString, header.Username, header.Password)))
                {
                   List<String> managerRoles = new List<string>();

                    List<DATA_MANAGER> ManagerList = aLDSE.DATA_MANAGER.AsEnumerable()
                                    .Where(manager => manager.USERNAME.ToUpper() == header.Username.ToUpper()).ToList();
                    ManagerList.ForEach(m => managerRoles.Add(m.ROLE.ROLE_NAME));

                    if (ManagerList.Any())
                    {
                        return new AuthenticationResult.Success(header.Username, managerRoles.ToArray());
                    }
                    else
                    {
                        return new AuthenticationResult.Failed();
                    }

                }//end using
            } catch (EntityException)
            {
                return new AuthenticationResult.Failed();
            }
        }

        //Add public method for decoding base 64 later
        public static BasicAuthRequestHeader ExtractBasicHeader(string value)
        {
            try
            {
                var basicBase64Credentials = value.Split(' ')[1];

                var basicCredentials = Encoding.UTF8.GetString( Convert.FromBase64String( basicBase64Credentials ) ).Split(':');

                if (basicCredentials.Length != 2)
                    return null;

                return new BasicAuthRequestHeader(basicCredentials[0], basicCredentials[1]);
            }
            catch (Exception ex)
            {
                return null;
            }

        }

    }
}