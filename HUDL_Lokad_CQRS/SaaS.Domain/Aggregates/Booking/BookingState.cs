using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SaaS.Aggregates.Booking
{
    public sealed class BookingState : IBookingState
    {
        public SecurityId SecurityId { get; private set; }
        public string LockoutMessage { get; private set; }
        public BookId Id { get; private set; }

        public Rental Rent { get; private set; }
        public void When(BookingCreated e)
        {
            Id = e.Id;
            Rent = e.Rent;
        }

        public BookingState(IEnumerable<IEvent> events)
        {
         
            foreach (var e in events)
            {
                Mutate(e);
            }
        }

        public int Version { get; private set; }

        public void Mutate(IEvent e)
        {
            Version += 1;
            RedirectToWhen.InvokeEventOptional(this, e);
        }
    }
}
