using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OMNI.CustomControls
{
    /// <summary>
    /// Command ComboBox Control
    /// </summary>
    public class CommandComboBox : ComboBox, ICommandSource
    {
        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(CommandComboBox), new UIPropertyMetadata(null));

        public IInputElement CommandTarget
        {
            get { return (IInputElement)GetValue(CommandTargetProperty); }
            set { SetValue(CommandTargetProperty, value); }
        }

        public static readonly DependencyProperty CommandTargetProperty =
            DependencyProperty.Register(nameof(CommandTarget), typeof(IInputElement), typeof(CommandComboBox), new UIPropertyMetadata(null));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(CommandComboBox),
            new PropertyMetadata(null, new PropertyChangedCallback(CommandChanged)));

        public static void CommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var cc = (CommandComboBox)d;
            cc.HoopUpCommand((ICommand)e.OldValue, (ICommand)e.NewValue);
        }
        public void HoopUpCommand(ICommand oldCommand, ICommand newCommand)
        {
            if (oldCommand != null) RemoveCommand(oldCommand);
            AddCommand(newCommand);
        }
        public void RemoveCommand(ICommand oldCommand)
        {
            EventHandler handler = OnCanExecuteChanged;
            oldCommand.CanExecuteChanged -= handler;
        }
        public void AddCommand(ICommand newCommand)
        {
            canExecuteChangedHandler = new EventHandler(OnCanExecuteChanged);
            if (newCommand != null) newCommand.CanExecuteChanged += canExecuteChangedHandler;
        }
        void OnCanExecuteChanged(object sender, EventArgs e)
        {
            if (Command != null)
            {
                var command = Command as RoutedCommand;
                IsEnabled = command != null ? command.CanExecute(CommandParameter, CommandTarget) ? true : false : Command.CanExecute(CommandParameter) ? true : false;
            }
        }
        protected override void OnDropDownOpened(EventArgs e)
        {
            base.OnDropDownOpened(e);
            if (Command != null)
            {
                if (Command is RoutedCommand command)
                    command.Execute(CommandParameter, CommandTarget);
                else
                    Command.Execute(CommandParameter);
            }
        }
        private EventHandler canExecuteChangedHandler;

    }
}
