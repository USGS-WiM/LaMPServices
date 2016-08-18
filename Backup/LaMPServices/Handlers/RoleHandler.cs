//------------------------------------------------------------------------------
//----- RoleHandler -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Handles Role type resources through the HTTP uniform interface.
//              Equivalent to the controller in MVC.
//
//discussion:   Handlers are objects which handle all interaction with resources in 
//              this case the resources are POCO classes derived from the EF. 
//               https://github.com/openrasta/openrasta/wiki/Handlers
//
//     
#region Comments
// 03.11.13 - jkn - Created
#endregion


using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using LaMPServices.Authentication;
using OpenRasta.Web;
using OpenRasta.Security;

namespace LaMPServices.Handlers
{
    public class RoleHandler : HandlerBase
    {
        #region Routed Methods

        #region GetMethods
        [RequiresAuthentication]
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<ROLE> roleList;

            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    roleList = aLaMPRDS.ROLEs.OrderBy(r => r.ROLE_ID).ToList();

                    if (roleList != null)
                        roleList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));


                }//end using
            }//end using

            return new OperationResult.OK { ResponseResource = roleList };
        }//end httpMethod get

        [RequiresAuthentication]
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 roleId)
        {
            ROLE aRole;

            //return BadRequest if ther is no ID
            if (roleId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    aRole = aLaMPRDS.ROLEs.SingleOrDefault(r => r.ROLE_ID == roleId);

                    if (aRole != null)
                        aRole.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using
            }//end using

            return new OperationResult.OK { ResponseResource = aRole };
        }//end httpMethod get

         [RequiresAuthentication]
        [HttpOperation(HttpMethod.GET, ForUriName = "GetDataManagersRole")]
        public OperationResult GetDataManagersRole(Int32 dataManagerId)
        {
            ROLE aRole;

            //return BadRequest if ther is no ID
            if (dataManagerId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    aRole = aLaMPRDS.DATA_MANAGER.SingleOrDefault(dm => dm.DATA_MANAGER_ID == dataManagerId).ROLE;

                    if (aRole != null)
                        aRole.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using
            }//end using

            return new OperationResult.OK { ResponseResource = aRole };
        }//end httpMethod get

        #endregion
        #endregion

    }//end StatusTypeHandler
}//end namespace