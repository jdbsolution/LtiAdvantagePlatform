﻿using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using LtiAdvantage.AssignmentGradeServices;
using LtiAdvantage.NamesRoleProvisioningService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;

namespace AdvantagePlatform.Controllers
{
    /// <inheritdoc />
    /// <summary>
    /// Sample membership controller that implements the Membership service.
    /// See https://www.imsglobal.org/spec/lti-nrps/v2p0.
    /// </summary>
    public class MembershipController : MembershipControllerBase
    {
        private readonly ApplicationDbContext _appContext;

        public MembershipController(ILogger<MembershipControllerBase> logger, ApplicationDbContext appContext) : base(logger)
        {
            _appContext = appContext;
        }

        /// <summary>
        /// Sample implementation of OnGetMembershipAsync returns both members of the
        /// sample course. This sample ignores limit, rlid, and role parameters.
        /// </summary>
        /// <param name="request">The <see cref="GetMembershipRequest"/> including the course id.</param>
        /// <returns>The members of the sample course.</returns>
        protected override async Task<MembershipContainerResult> OnGetMembershipAsync(GetMembershipRequest request)
        {
            var result = new MembershipContainerResult(StatusCodes.Status200OK);

            var course = await _appContext.Courses.FindAsync(request.ContextId);
            if (course == null)
            {

                result.StatusCode = StatusCodes.Status404NotFound;
                return result;
            }

            var user = await _appContext.GetUserAsync(course.UserId);
            if (user == null)
            {
                result.StatusCode = StatusCodes.Status404NotFound;
                return result;
            }

            result.MembershipContainer = new MembershipContainer()
            {
                Id = Request.GetDisplayUrl()
            };

            var people = user.People;
            if (people.Any())
            {
                result.MembershipContainer.Members = people
                    .Select(p => new Member
                    {
                        ContextId = course.Id,
                        ContextTitle = course.Name,
                        FamilyName = p.LastName,
                        GivenName = p.FirstName,
                        Roles = Areas.Identity.Pages.Account.Manage.PeopleModel.ParsePersonRoles(p.Roles),
                        Status = MemberStatus.Active,
                        LisPersonSourcedId = p.SisId,
                        UserId = p.Id
                    })
                    .ToList();
            }

            return result;
        }
    }
}