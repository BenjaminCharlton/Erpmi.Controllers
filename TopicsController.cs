using Erpmi.Core;
using Erpmi.Core.ViewModels.Topics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using Basics;
using Basics.UI;
using Erpmi.Core.Models;
using System.Linq;
using Basics.Configuration;
using Basics.Mvc;
using System;

namespace Erpmi.Controllers
{
    [Authorize]
    public class TopicsController : ApplicationController
    {
        public TopicsController(
            IOptions<OrganizationDetailsOptions> organizationOptions,
            IUnitOfWork unitOfWork,
            IAuthorizationService authorizationService) : base(organizationOptions, unitOfWork, authorizationService) { }

        [HttpPost]
        [HttpGet]
        public IActionResult Index(int id)// The exam Id!
        {
            if (id <= 0)
                return RedirectToAction(nameof(ExamsController.Index), nameof(ExamsController).ControllerName());

            var model = new EditViewModel()
            {
                Title = GetFromTempData(nameof(EditViewModel.Title), "Edit topics"),
                StatusMessage = GetFromTempData<Alert>(nameof(EditViewModel.StatusMessage)),
                ExamId = id,
                Topics = UnitOfWork.Topics.GetTopics(id)
            };

            return View(model);
        }


        [HttpGet]
        public IActionResult Delete(int id) // The topic Id!
        {
            IActionResult result = null;
            return result

            .RedirectWithErrorIfEntityDoesNotExist(
                id,
                UnitOfWork.Topics,
                out var topic,
                nameof(ExamsController.Index),
                nameof(ExamsController).ControllerName())

            .RedirectWithErrorIfNot(
                topic.Exam.CreatedByUser.Equals(CurrentUser),
                "You don't have permission to delete topics from that exam.",
                nameof(ExamsController.Index),
                nameof(ExamsController).ControllerName())

            .RedirectWithErrorIf(topic.PossibleQuestions.Any(),
                "You can't delete that topic while it still has questions! Delete the questions " +
                "or move them into another topic, then try again.",
                nameof(ExamsController.Edit),
                nameof(ExamsController).ControllerName(),
                topic.Exam.Id)

            .RedirectWithErrorIfActionFails(
                () =>
                {
                    UnitOfWork.Topics.Remove(id);
                    UnitOfWork.Complete();
                },
                "That topic could not be deleted.",
                nameof(Index),
                string.Empty,
                topic.Exam.Id)

            .RedirectWithSuccess("That topic has been updated!",
                nameof(Index),
                string.Empty,
                topic.Exam.Id);
        }

        [HttpGet]
        public IActionResult Edit(int id) //The topic Id!
        {
            IActionResult result = null;
            return result

            .RedirectWithErrorIfEntityDoesNotExist(
                id,
                UnitOfWork.Topics,
                out var topic,
                nameof(ExamsController.Index),
                nameof(ExamsController).ControllerName())

            .RedirectWithErrorIfNot(
                topic.Exam.CreatedByUser.Equals(CurrentUser),
                nameof(ExamsController.Index),
                nameof(ExamsController).ControllerName(),
                "You don't have permission to edit the requirements of that exam.")

            .View(new EditViewModel()
            {
                Topics = UnitOfWork.Topics.GetTopics(topic.Exam),
                EditingTopicId = topic.Id,
                ExamId = topic.Exam.Id,
                EditingTopicName = topic.Name,
                EditingTopicDescription = topic.Description,
                EditingTopicNumberOfQuestionsToBeAttempted = topic.NumberOfQuestionsToBeAttempted,
                Title = "Edit Topics"
            });
        }


        [HttpPost]
        public IActionResult Edit(EditViewModel model)
        {
            IActionResult result = null;
            return result

            .RedirectWithErrorIfEntityDoesNotExist(
                model.EditingTopicId,
                UnitOfWork.Topics,
                out var topic,
                nameof(Edit),
                string.Empty,
                string.Empty,
                model.ExamId)

            .RedirectWithErrorIfNot(
                topic.Exam.CreatedByUser.Equals(CurrentUser),
                nameof(Edit),
                string.Empty,
                "You don't have permission to edit the topics of that exam.")

            .RedirectWithErrorIf(IsDuplicateTopic(model.EditingTopicName, topic.Id, topic.Exam),
                "Be original! That exam already has a topic with that name!",
                nameof(Edit),
                string.Empty,
                model.ExamId)

            .RedirectWithErrorIfActionFails(
                () =>
                {
                    topic.Update(
                        CurrentUser,
                        model.EditingTopicName,
                        model.EditingTopicDescription,
                        model.EditingTopicNumberOfQuestionsToBeAttempted);
                    UnitOfWork.Complete();
                },
                "That topic could not be updated",
                nameof(Edit),
                string.Empty,
                model.ExamId)

            .RedirectWithSuccess(
                "That topic has been updated!",
                nameof(Edit), "", model.ExamId);
        }

        [HttpPost]
        public IActionResult Add(EditViewModel model)
        {
            IActionResult result = null;
            return result

            .RedirectWithErrorIfEntityDoesNotExist(
                model.ExamId,
                UnitOfWork.Exams,
                out var exam,
                nameof(ExamsController.Index),
                nameof(ExamsController).ControllerName())

            .RedirectWithErrorIfNot(
                exam.CreatedByUser.Equals(CurrentUser),
                nameof(ExamsController.Index),
                nameof(ExamsController).ControllerName(),
                "You don't have permission to edit the topics of that exam.")

            .RedirectWithErrorIf(IsTopicOfExam(model.EditingTopicName, exam),
                "Be original! That exam already has a topic with that name!",
                nameof(Edit),
                string.Empty,
                model.ExamId)

            .RedirectWithErrorIfActionFails(
                () =>
                {
                    exam.AddTopic(
                        CurrentUser,
                        model.EditingTopicName,
                        model.EditingTopicDescription,
                        model.EditingTopicNumberOfQuestionsToBeAttempted);

                    UnitOfWork.Complete();
                },
                "The new topic could not be added to that exam.",
                  nameof(Edit), 
                  string.Empty,
                  model.ExamId)

            .RedirectWithSuccess("The new topic has been added!",
                nameof(Edit),
                string.Empty,
                model.ExamId);
        }

        private bool IsDuplicateTopic(string suggestedName, int editingTopicId, Exam exam)
        {
            return UnitOfWork.Topics.IsDuplicateName(suggestedName, editingTopicId);
        }

        private bool IsTopicOfExam(string name, Exam exam)
        {
            return UnitOfWork.Topics.IsTopicOfExam(name, exam);
        }
    }
}
