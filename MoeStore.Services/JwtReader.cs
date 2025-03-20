using MoeStore.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MoeStore.Services
{
    public class JwtReader
    {
        public static int GetUserId(ClaimsPrincipal user)
        {
            var identity = user.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return 0;
            }
            // read teh claim
            var claim = identity.Claims.FirstOrDefault(c => c.Type.ToLower() == "id");
            if (claim == null)
            {
                return 0;
            }
            // convert the value of claim to integer
            int id;
            try
            {
                id = int.Parse(claim.Value);
            }
            catch (Exception)
            {

                return 0;
            }
            return id;
        }

        public static string GetUserRole(ClaimsPrincipal user)
        {
            var identity = user.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return "";
            }
            // read teh claim
            var claim = identity.Claims.FirstOrDefault(c => c.Type.ToLower().Contains("role"));
            if (claim == null)
            {
                return "";
            }

            return claim.Value;
        }

        public static Dictionary<string, string> GetUserClaims(ClaimsPrincipal user)
        {
            Dictionary<string, string> claims = new Dictionary<string, string>();

            var identity = user.Identity as ClaimsIdentity;

            if (identity != null)
            {
                foreach (var claim in identity.Claims)
                {
                    claims.Add(claim.Type, claim.Value);
                }
            }

            return claims;
        }
    }
}
