using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Microsoft.FamilyShow.Messages
{
    public class UpdateStatusMessage : ValueChangedMessage<string>
    {
        public UpdateStatusMessage(string message) : base(message) {}
    }
}
