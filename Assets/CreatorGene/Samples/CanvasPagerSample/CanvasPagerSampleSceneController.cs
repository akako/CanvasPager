using System.Collections;
using System.Collections.Generic;
using CreatorGene.UI;
using UnityEngine;
using UnityEngine.UI;

namespace CreatorGene.Samples.CanvasPagerSample
{
    public class CanvasPagerSampleSceneController : CanvasPager
    {
        [SerializeField] private Button _showPage1Button;

        protected override void Start()
        {
            base.Start();
            
            _showPage1Button.onClick.AddListener(() =>
            {
                Show("CanvasPagerSamplePage1");
            });
        }
    }
}