using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FM4CC.FaultModels.AllowedOscillation
{
    internal class TestGenerationViewModel
    {
        public ICollectionView StepTopExplorationResults { get; private set; }
        public ICollectionView StepBottomExplorationResults { get; private set; }
        public ICollectionView SineExplorationResults { get; private set; }
        public string DescriptionText { get; private set; }

        internal TestGenerationViewModel(List<AllowedOscillationInternalTestCase> stepTopExplorationResults, List<AllowedOscillationInternalTestCase> stepBottomExplorationResults, List<AllowedOscillationInternalTestCase> sineExplorationResults)
        {
            StepTopExplorationResults = CollectionViewSource.GetDefaultView(stepTopExplorationResults);
            StepBottomExplorationResults = CollectionViewSource.GetDefaultView(stepBottomExplorationResults);
            SineExplorationResults = CollectionViewSource.GetDefaultView(sineExplorationResults);
        }
    }
}
