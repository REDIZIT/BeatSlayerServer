using BeatSlayerServer.Models.Database;
using BeatSlayerServer.Utils;
using System.Linq;

namespace BeatSlayerServer.Services.Game
{
    public class VerificationService
    {
        private readonly MyDbContext ctx;

        public VerificationService(MyDbContext ctx)
        {
            this.ctx = ctx;
        }

        public void Send(CodeVerificationRequest request)
        {
            ctx.VerificationRequests.RemoveRange(ctx.VerificationRequests.Where(c => c.Nick == request.Nick));
            ctx.VerificationRequests.Add(request);
            ctx.SaveChanges();
        }

        public void Remove(CodeVerificationRequest request)
        {
            ctx.VerificationRequests.Remove(request);
            ctx.SaveChanges();
        }


        public void Send(string nick, string value, string code)
        {
            Send(new CodeVerificationRequest(nick, value, code));
        }
    }
}
