﻿using System;
using System.ComponentModel;

namespace UE4PakEditor.ViewModel
{
    internal class ImportByParameterCommand : AbstractWorkerCommand
    {
        private OneTimeRunViewModel myOneTimeRunViewModel;

        public override void Execute(object parameter)
        {
            myOneTimeRunViewModel = (OneTimeRunViewModel)parameter;
            Worker.RunWorkerAsync();
        }

        protected override void DoWork(object sender, DoWorkEventArgs e)
        {
            myOneTimeRunViewModel.Import();
            myOneTimeRunViewModel.OnRequestClose(new EventArgs());
        }
    }
}
