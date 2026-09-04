using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Infrustructure.Identity.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(ApplicationUser user, IList<string> roles);
    }
}
