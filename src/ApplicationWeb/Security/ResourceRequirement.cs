using Microsoft.AspNetCore.Authorization;

namespace ApplicationWeb.Security
{
    public enum ResourceAction
    {
        Create,
        Edit,
        Delete,
        View
    }
    public class ResourceRequirement : IAuthorizationRequirement
    {
        public ResourceRequirement(ResourceAction action)
        {
            Action = action;
        }

        public ResourceAction Action { get; }
    }

    public class ResourceAuthorizationHandler : AuthorizationHandler<ResourceRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ResourceRequirement requirement)
        {
            if (context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
            }

            // TODO: Check if user has permission to perform the requested action

            return Task.CompletedTask;
        }
    }
}
