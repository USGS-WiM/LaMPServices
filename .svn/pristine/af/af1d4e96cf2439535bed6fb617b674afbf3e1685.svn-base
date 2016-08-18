//------------------------------------------------------------------------------
//----- LaMPRequiresRoleAttribute -----------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Sets role attribute to be used by roleInterceptor  
//
//discussion:   
//
//     

#region Comments
// 03.07.13 - jkn - Adapted from STN
// 07.03.12 - jkn - Created from openrasta.security.RequriesRoleAttribute.cs

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OpenRasta.DI;
using OpenRasta.OperationModel;
using OpenRasta.OperationModel.Interceptors;
using OpenRasta.Security;

namespace LaMPServices.Authentication
{
    public class LaMPRequiresRoleAttribute: OpenRasta.OperationModel.Interceptors.InterceptorProviderAttribute
    {
        readonly List<string> _roleNames = new List<string>();
       
        #region Constructors
        public LaMPRequiresRoleAttribute(string roleName)
        {
            if (roleName == null) throw new ArgumentNullException("roleName");
                _roleNames.Add(roleName);
        }

        /// <summary>
        /// overloaded constructor that has a specified set of roles
        /// </summary>
        /// <param name="roleNames"></param>
        public LaMPRequiresRoleAttribute(string[] roleNames)
        {

            foreach (string role in roleNames)
            {
                _roleNames.Add(role);

            }//next

            if (_roleNames.Count < 0) throw new ArgumentNullException("roleNames");
          
        }

        #endregion

        public override IEnumerable<IOperationInterceptor> GetInterceptors(IOperation operation)
        {
            yield return DependencyManager.GetService<RequiresAuthenticationInterceptor>();
            var roleInterceptor = DependencyManager.GetService<LaMPRequiresRoleInterceptor>();
            roleInterceptor.Roles = _roleNames;
            yield return roleInterceptor;
        }

    }//end class RequiresRolesAttribute
}//end namespace