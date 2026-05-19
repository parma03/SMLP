/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

using Psmp.Base.Selections.Enums;
using Psmp.Bsui.Features.Applicants.Selection.Models;

namespace Psmp.Bsui.Features.Applicants.Selection.Constants;

public static class SelectionTimelineItem
{
    private static List<SelectionTimelineModel> TimelineItem { get; set; } = new();

    public static class Administration
    {
        public static IList<SelectionTimelineModel> InProgress()
        {
            TimelineItem = new();
            TimelineItem.Add(TimelineItemAdministration.InProgress);
            TimelineItemEmpyt();
            return TimelineItem;
        }

        public static IList<SelectionTimelineModel> Passed()
        {
            TimelineItem = new();
            TimelineItem.Add(TimelineItemAdministration.Passed);
            TimelineItemEmpyt();
            return TimelineItem;
        }

        public static IList<SelectionTimelineModel> Rejected()
        {
            TimelineItem = new();
            TimelineItem.Add(TimelineItemAdministration.Rejected);
            TimelineItemEmpyt();
            return TimelineItem;
        }

        private static void TimelineItemEmpyt()
        {
            TimelineItem.Add(TimelineItemToefl.Empty);
            TimelineItem.Add(TimelineItemPsychotest.Empty);
            TimelineItem.Add(TimelineItemTalentReview.Empty);
        }
    }

    public static class TalentReview1st
    {
        public static IList<SelectionTimelineModel> InProgress()
        {
            TimelineItem = new();
            TimelineItem.Add(TimelineItemTalentReview1st.InProgress);
            TimelineItemEmpyt();
            return TimelineItem;
        }

        public static IList<SelectionTimelineModel> Passed()
        {
            TimelineItem = new();
            TimelineItem.Add(TimelineItemTalentReview1st.Passed);
            TimelineItemEmpyt();
            return TimelineItem;
        }

        public static IList<SelectionTimelineModel> Rejected()
        {
            TimelineItem = new();
            TimelineItem.Add(TimelineItemTalentReview1st.Rejected);
            TimelineItemEmpyt();
            return TimelineItem;
        }

        private static void TimelineItemEmpyt()
        {
            TimelineItem.Add(TimelineItemToefl.Empty);
            TimelineItem.Add(TimelineItemPsychotest.Empty);
            TimelineItem.Add(TimelineItemTalentReview.Empty);
        }
    }

    public static class Toefl
    {
        private static void AdministrationTalentReview1st_Passed(SelectionType selectionType)
        {
            if (selectionType == 0)
            {
                TimelineItem.Add(TimelineItemTalentReview1st.Passed);
            }
            else
            {
                TimelineItem.Add(TimelineItemAdministration.Passed);
            }
        }

        private static void TalentReview_Empty(SelectionType selectionType)
        {
            TimelineItem.Add(TimelineItemPsychotest.Empty);
            if (selectionType == 0)
            {
                TimelineItem.Add(TimelineItemTalentReview2nd.Empty);
            }
            else
            {
                TimelineItem.Add(TimelineItemTalentReview.Empty);
            }
        }

        public static IList<SelectionTimelineModel> InProgress(SelectionType selectionType)
        {
            TimelineItem = new();
            AdministrationTalentReview1st_Passed(selectionType);
            TimelineItem.Add(TimelineItemToefl.InProgress);
            TalentReview_Empty(selectionType);
            return TimelineItem;
        }

        public static IList<SelectionTimelineModel> Passed(SelectionType selectionType)
        {
            TimelineItem = new();
            AdministrationTalentReview1st_Passed(selectionType);
            TimelineItem.Add(TimelineItemToefl.Passed);
            TalentReview_Empty(selectionType);
            return TimelineItem;
        }

        public static IList<SelectionTimelineModel> Rejected(SelectionType selectionType)
        {
            TimelineItem = new();
            AdministrationTalentReview1st_Passed(selectionType);
            TimelineItem.Add(TimelineItemToefl.Rejected);
            TalentReview_Empty(selectionType);
            return TimelineItem;
        }

        public static IList<SelectionTimelineModel> Failed(SelectionType selectionType)
        {
            TimelineItem = new();
            AdministrationTalentReview1st_Passed(selectionType);
            TimelineItem.Add(TimelineItemToefl.Failed);
            TalentReview_Empty(selectionType);
            return TimelineItem;
        }
    }

    public static class Psychotest
    {
        private static void Toefl_Passed(SelectionType selectionType)
        {
            if (selectionType == 0)
            {
                TimelineItem.Add(TimelineItemTalentReview1st.Passed);
            }
            else
            {
                TimelineItem.Add(TimelineItemAdministration.Passed);
            }

            TimelineItem.Add(TimelineItemToefl.Passed);
        }

        private static void TalentReview_Empty(SelectionType selectionType)
        {
            if (selectionType == 0)
            {
                TimelineItem.Add(TimelineItemTalentReview2nd.Empty);
            }
            else
            {
                TimelineItem.Add(TimelineItemTalentReview.Empty);
            }
        }

        public static IList<SelectionTimelineModel> InProgress(SelectionType selectionType)
        {
            TimelineItem = new();
            Toefl_Passed(selectionType);
            TimelineItem.Add(TimelineItemPsychotest.InProgress);
            TalentReview_Empty(selectionType);
            return TimelineItem;
        }

        public static IList<SelectionTimelineModel> Passed(SelectionType selectionType)
        {
            TimelineItem = new();
            Toefl_Passed(selectionType);
            TimelineItem.Add(TimelineItemPsychotest.Passed);
            TalentReview_Empty(selectionType);
            return TimelineItem;
        }

        public static IList<SelectionTimelineModel> Rejected(SelectionType selectionType)
        {
            TimelineItem = new();
            Toefl_Passed(selectionType);
            TimelineItem.Add(TimelineItemPsychotest.Rejected);
            TalentReview_Empty(selectionType);
            return TimelineItem;
        }

        public static IList<SelectionTimelineModel> Failed(SelectionType selectionType)
        {
            TimelineItem = new();
            Toefl_Passed(selectionType);
            TimelineItem.Add(TimelineItemPsychotest.Failed);
            TalentReview_Empty(selectionType);
            return TimelineItem;
        }
    }

    public static class TalentReview
    {
        private static void Psychotest_Passed()
        {
            TimelineItem.Add(TimelineItemAdministration.Passed);
            TimelineItem.Add(TimelineItemToefl.Passed);
            TimelineItem.Add(TimelineItemPsychotest.Passed);
        }

        public static IList<SelectionTimelineModel> InProgress()
        {
            TimelineItem = new();
            Psychotest_Passed();
            TimelineItem.Add(TimelineItemTalentReview.InProgress);
            return TimelineItem;
        }

        public static IList<SelectionTimelineModel> Failed()
        {
            TimelineItem = new();
            Psychotest_Passed();
            TimelineItem.Add(TimelineItemTalentReview.Failed);
            return TimelineItem;
        }

        public static IList<SelectionTimelineModel> Passed()
        {
            TimelineItem = new();
            Psychotest_Passed();
            TimelineItem.Add(TimelineItemTalentReview.Passed);
            return TimelineItem;
        }

        public static IList<SelectionTimelineModel> Rejected()
        {
            TimelineItem = new();
            Psychotest_Passed();
            TimelineItem.Add(TimelineItemTalentReview.Rejected);
            return TimelineItem;
        }
    }

    public static class TalentReview2nd
    {
        private static void Psychotest_Passed()
        {
            TimelineItem.Add(TimelineItemTalentReview1st.Passed);
            TimelineItem.Add(TimelineItemToefl.Passed);
            TimelineItem.Add(TimelineItemPsychotest.Passed);
        }

        public static IList<SelectionTimelineModel> InProgress()
        {
            TimelineItem = new();
            Psychotest_Passed();
            TimelineItem.Add(TimelineItemTalentReview2nd.InProgress);
            return TimelineItem;
        }

        public static IList<SelectionTimelineModel> Passed()
        {
            TimelineItem = new();
            Psychotest_Passed();
            TimelineItem.Add(TimelineItemTalentReview2nd.Passed);
            return TimelineItem;
        }

        public static IList<SelectionTimelineModel> Rejected()
        {
            TimelineItem = new();
            Psychotest_Passed();
            TimelineItem.Add(TimelineItemTalentReview2nd.Rejected);
            return TimelineItem;
        }
    }
}
