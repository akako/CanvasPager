using System.Collections;
using System.Collections.Generic;
using CreatorGene.UI;
using UnityEngine;
using UnityEngine.UI;


namespace CreatorGene.Samples.CanvasPagerSample
{
    public class CanvasPagerSamplePage1 : CanvasPager
    {
        [SerializeField] private Button _showPage2Button;

        protected override void Start()
        {
            base.Start();
            
            _showPage2Button.onClick.AddListener(() =>
            {
                Show("CanvasPagerSamplePage2");
            });
        }
    }
}