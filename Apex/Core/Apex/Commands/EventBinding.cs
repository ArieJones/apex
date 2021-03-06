﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Reflection;
using Apex.Consistency;

namespace Apex.Commands
{
    /// <summary>
    /// Provides a binding mechanism between a named event and a command.
    /// </summary>
    public class EventBinding : Freezable
    {
        /// <summary>
        /// When implemented in a derived class, creates a new instance of the <see cref="T:System.Windows.Freezable"/> derived class.
        /// </summary>
        /// <returns>
        /// The new instance.
        /// </returns>
        protected override Freezable CreateInstanceCore()
        {
            return new EventBinding();
        }

        /// <summary>
        /// The EventName Dependency Property.
        /// </summary>
        private static readonly DependencyProperty EventNameProperty =
          DependencyProperty.Register("EventName", typeof(string), typeof(EventBinding),
          new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the name of the event.
        /// </summary>
        /// <value>
        /// The name of the event.
        /// </value>
        public string EventName
        {
            get { return (string)GetValue(EventNameProperty); }
            set { SetValue(EventNameProperty, value); }
        }

        /// <summary>
        /// The Command Dependency property.
        /// </summary>
        public static readonly DependencyProperty CommandProperty =
          DependencyProperty.Register("Command", typeof(ICommand), typeof(EventBinding),
          new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the command.
        /// </summary>
        /// <value>
        /// The command.
        /// </value>
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        /// <summary>
        /// The command parameter dependency property.
        /// </summary>
        public static readonly DependencyProperty CommandParameterProperty =
          DependencyProperty.Register("CommandParameter", typeof(object), typeof(EventBinding),
          new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the command parameter.
        /// </summary>
        /// <value>
        /// The command parameter.
        /// </value>
        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        /// <summary>
        /// Binds the specified o.
        /// </summary>
        /// <param name="o">The o.</param>
        public void Bind(object o)
        {
            try
            {

                //  Get the event info from the event name.
                EventInfo eventInfo = o.GetType().GetEvent(EventName);

                //  Get the method info for the event proxy.
                MethodInfo methodInfo = GetType().GetMethod("EventProxy", BindingFlags.NonPublic | BindingFlags.Instance);

                //  Create a delegate for the event to the event proxy.
                Delegate del = Delegate.CreateDelegate(eventInfo.EventHandlerType, this, methodInfo);

                //  Add the event handler. (Removing it first if it already exists!)
                eventInfo.RemoveEventHandler(o, del);
                eventInfo.AddEventHandler(o, del);
            }
            catch (Exception e)
            {
                string s = e.ToString();
            }
        }

        /// <summary>
        /// Proxy to actually fire the event.
        /// </summary>
        /// <param name="o">The object.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void EventProxy(object o, EventArgs e)
        {   
#if SILVERLIGHT

            //  If we're in Silverlight, we have NOT inherited the data context
            //  because the EventBindingCollection is not a framework element and
            //  therefore out of the logical tree. However, we can set it here 
            //  and update the bindings - and it will all work.
            DataContext = ParentElement != null ? ParentElement.DataContext : null;
            var bindingExpression = GetBindingExpression(EventBinding.CommandProperty);
            if(bindingExpression != null)
                bindingExpression.UpdateSource();
            bindingExpression = GetBindingExpression(EventBinding.CommandParameterProperty);
            if (bindingExpression != null)
                bindingExpression.UpdateSource();

#endif

            if (Command != null)
                Command.Execute(CommandParameter);
        }

#if SILVERLIGHT
        
        /// <summary>
        /// Gets or sets the parent element. Only needed as a helper property in Silverlight.
        /// </summary>
        /// <value>
        /// The parent element.
        /// </value>
        public FrameworkElement ParentElement
        {
            get;
            set;
        }

#endif
    }
}
