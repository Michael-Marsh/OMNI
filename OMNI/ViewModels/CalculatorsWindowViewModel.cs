using OMNI.Commands;
using OMNI.Helpers;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace OMNI.ViewModels
{
    /// <summary>
    /// Calculators Window ViewModel Interaction Logic
    /// </summary>
    public class CalculatorsWindowViewModel : ViewModelBase
    {
        #region Properties

        //Header Calc() Properties
        private string bladeToPin;
        public string BladeToPin
        {
            get { return bladeToPin; }
            set { bladeToPin = value; OnPropertyChanged(nameof(BladeToPin)); }
        }
        private string previousBP;
        public string PreviousBP
        {
            get { return previousBP; }
            set { previousBP = value; OnPropertyChanged(nameof(PreviousBP)); }
        }
        private string headerLength;
        public string HeaderLength
        {
            get { return headerLength; }
            set { headerLength = value; OnPropertyChanged(nameof(headerLength)); }
        }

        //SBC() Properites
        public double? CtoC { get; set; }
        public double? LRDiam { get; set; }
        public double? SRDiam { get; set; }
        public double TestCtoC { get; set; }

        //Blade To Slat Calc() Properties
        private string specLength;
        public string SpecLength
        {
            get { return specLength; }
            set { specLength = value; OnPropertyChanged(nameof(SpecLength)); }
        }
        private string previousSpec;
        public string PreviousSpec
        {
            get { return previousSpec; }
            set { previousSpec = value; OnPropertyChanged(nameof(PreviousSpec)); }
        }
        private string btoSLength;
        public string BtoSLength
        {
            get { return btoSLength; }
            set { btoSLength = value; OnPropertyChanged(nameof(BtoSLength)); }
        }
        private string headLength;
        public string HeadLength
        {
            get { return headLength; }
            set { headLength = value; OnPropertyChanged(nameof(HeadLength)); }
        }

        RelayCommand _calc;

        #endregion

        /// <summary>
        /// Calculators Window ViewModel Constructor
        /// </summary>
        public CalculatorsWindowViewModel()
        {

        }

        /// <summary>
        /// Calculate a Head and Tail length for Cut-Offs based on a part number
        /// </summary>
        public void HeaderCalc()
        {
            using (BackgroundWorker bw = new BackgroundWorker())
            {
                try
                {
                    HeaderLength = "Calculating...";
                    PreviousBP = BladeToPin;
                    BladeToPin = string.Empty;
                    bw.DoWork += new DoWorkEventHandler(
                        delegate (object sender, DoWorkEventArgs e)
                        {
                            var descLength = Convert.ToDouble(previousBP);
                            if (descLength > 0)
                            {
                                var cleatSpaceCount = Math.Truncate(descLength / 12);
                                if (((descLength - 12 * cleatSpaceCount) / 2) < 5.100)
                                {
                                    cleatSpaceCount -= 1;
                                }
                                HeaderLength = cleatSpaceCount * 12 == descLength ? "6" : ((descLength - 12 * cleatSpaceCount) / 2).ToString();
                                if (!HeaderLength.Equals("0.00") || !HeaderLength.Equals(Convert.ToInt32(HeaderLength).ToString()))
                                {
                                    var _headerDouble = Convert.ToDouble(HeaderLength);
                                    var _headerInt = Math.Floor(Convert.ToDouble(HeaderLength));
                                    HeaderLength =
                                        _headerInt + .25 > _headerDouble
                                            ? _headerInt.ToString()
                                            : _headerInt + .25 < _headerDouble && _headerInt + .75 > _headerDouble
                                                ? (_headerInt + .5).ToString()
                                                : (_headerInt + 1).ToString();
                                    if (HeaderLength.Equals("12"))
                                    {
                                        HeaderLength = "6";
                                    }
                                }
                            }
                            else
                            {
                                HeaderLength = "Invalid";
                            }
                        });
                    bw.RunWorkerAsync();
                }
                catch (Exception ex)
                {
                    ExceptionWindow.Show("Unhandled Exception", ex.Message, ex);
                }
            }
        }

        /// <summary>
        /// Simple Belt Calculator OMNI Implementation
        /// </summary>
        /// <returns>Belt Length</returns>
        public double SBC()
        {
            var _lrDiam = (double)LRDiam;
            var _srDiam = (double)SRDiam;
            var _ctoc = (double)CtoC;
            CtoC = LRDiam = SRDiam = null;
            OnPropertyChanged(nameof(CtoC));
            OnPropertyChanged(nameof(LRDiam));
            OnPropertyChanged(nameof(SRDiam));
            var degWrapLarge = Math.PI + (2 * (Math.Asin(((.5 * _lrDiam) - (.5 * _srDiam)) / _ctoc)));
            var degWrapSmall = Math.PI - (2 * (Math.Asin((((.5 * _lrDiam)) - (.5 * _srDiam)) / _ctoc)));
            var deltaYSmall = (_srDiam / 2) * Math.Sin((((.5 * _lrDiam) - (.5 * _srDiam)) / _ctoc));
            var deltaXSmall = (_srDiam / 2) - ((_srDiam / 2) * Math.Cos((Math.PI - degWrapSmall) / 2));
            var deltaYLarge = (_lrDiam / 2) * Math.Sin(((.5 * _lrDiam) - (.5 * _srDiam)) / _ctoc);
            var deltaXLarge = (_lrDiam / 2) - ((_lrDiam / 2) * Math.Cos((degWrapLarge - Math.PI) / 2));
            var lengthWrapLarge = (_lrDiam * Math.PI * (degWrapLarge / (2 * Math.PI)));
            var lengthWrapSmall = (_srDiam * Math.PI * (degWrapSmall / (2 * Math.PI)));
            var beltLength = ((_lrDiam * Math.PI * (degWrapLarge / (2 * Math.PI)) + (_srDiam * Math.PI * degWrapSmall / (2 * Math.PI)) + deltaXSmall - deltaXLarge) + (2 * (Math.Sqrt(Math.Pow((_ctoc - deltaYSmall + deltaYLarge), 2) - Math.Pow(((_lrDiam / 2) - (_srDiam / 2)), 2)))));
            return ((beltLength - (Math.PI * 101.6)) / 2);
        }

        /// <summary>
        /// Calculate a Blade to  length for Cut-Offs based on a Spec Length
        /// </summary>
        public void BladeToSlatCalc()
        {
            using (BackgroundWorker bw = new BackgroundWorker())
            {
                if (!string.IsNullOrEmpty(SpecLength))
                {
                    try
                    {
                        BtoSLength = "Calculating...";
                        PreviousSpec = SpecLength;
                        SpecLength = string.Empty;
                        bw.DoWork += new DoWorkEventHandler(
                            delegate (object sender, DoWorkEventArgs e)
                            {
                                var descLength = Convert.ToDouble(PreviousSpec);
                                if (descLength > 0)
                                {
                                    var cleatSpaceCount = Math.Truncate(descLength / 12);
                                    if (((descLength - 12 * cleatSpaceCount) / 2) < 5.000)
                                    {
                                        cleatSpaceCount = cleatSpaceCount - 1;
                                    }
                                    HeadLength = cleatSpaceCount * 12 == descLength ? "6" : ((descLength - 12 * cleatSpaceCount) / 2).ToString();
                                    if (!HeadLength.Equals("0.00") || !HeadLength.Equals(Convert.ToInt32(HeadLength).ToString()))
                                    {
                                        var _headerDouble = Convert.ToDouble(HeadLength);
                                        var _headerInt = Math.Floor(Convert.ToDouble(HeadLength));
                                        HeadLength =
                                            _headerInt + .25 > _headerDouble
                                                ? _headerInt.ToString()
                                                : _headerInt + .25 < _headerDouble && _headerInt + .75 > _headerDouble
                                                    ? (_headerInt + .5).ToString()
                                                    : (_headerInt + 1).ToString();
                                        if (HeadLength.Equals("12"))
                                        {
                                            HeadLength = "6";
                                        }
                                    }
                                    HeadLength = Math.Round(Convert.ToDouble(HeadLength), 1).ToString();
                                    BtoSLength = descLength < 156.00 ? Math.Round(descLength + 0.197 - Convert.ToDouble(HeadLength), 3).ToString() : Math.Round(descLength * 1.00125 - Convert.ToDouble(HeadLength), 3).ToString();
                                    BtoSLength = string.Format("{0:f3}", Convert.ToDouble(BtoSLength));
                                }
                                else
                                {
                                    HeadLength = "Invalid";
                                    BtoSLength = "Invalid";
                                }
                            });
                        bw.RunWorkerAsync();
                    }
                    catch (Exception ex)
                    {
                        ExceptionWindow.Show("Unhandled Exception", ex.Message, ex);
                    }
                }
                else
                {
                    BtoSLength = "Invalid";
                    HeadLength = "0.00";
                    PreviousSpec = string.Empty;
                    SpecLength = string.Empty;
                }
            }
        }

        #region View Commands

        public ICommand CalcCommand
        {
            get
            {
                if (_calc == null)
                {
                    _calc = new RelayCommand(CalcExecute, CalcCanExecute);
                }
                return _calc;
            }
        }

        private void CalcExecute(object parameter)
        {
            switch (Convert.ToInt32(parameter))
            {
                case 0:
                    HeaderCalc();
                    break;
                case 1:
                    TestCtoC = SBC();
                    OnPropertyChanged(nameof(TestCtoC));
                    break;
                case 2:
                    BladeToSlatCalc();
                    break;
            }
        }
        private bool CalcCanExecute(object parameter)
        {
            if (parameter != null && Convert.ToInt32(parameter) == 1)
            {
                return CtoC.HasValue && SRDiam.HasValue && LRDiam.HasValue ? true : false;
            }
            return true;
        }

        #endregion

        /// <summary>
        /// Object Disposal
        /// </summary>
        /// <param name="disposing">Called by the GC Finalizer</param>
        public override void OnDispose(bool disposing)
        {
            if (disposing)
            {

            }
        }
    }
}
