using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SaaS.Aggregates.Booking
{
    public sealed class BookingAggregate
    {

        readonly BookingState _state;
        public IList<IEvent> Changes = new List<IEvent>();

        /// <summary>
        /// If relogin happens within the interval, we don't track it
        /// </summary>
        public static readonly TimeSpan DefaultLoginActivityThreshold = TimeSpan.FromMinutes(10);

        public BookingAggregate(BookingState state)
        {
            _state = state;
        }

        public void Create(BookId userId, Rental rent)
        {
            if (_state.Version != 0)
                throw new DomainError("User already has non-zero version");

            Apply(new BookingCreated(userId, rent));
        }


        void Apply(IEvent<BookId> e)
        {
            _state.Mutate(e);
            Changes.Add(e);
        }
    }
}
