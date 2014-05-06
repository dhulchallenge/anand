using Lokad.Cloud;
using Lokad.Cloud.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SaaS.Aggregates.Booking
{
    public sealed class BookingApplicationService : IBookingApplicationService, IApplicationService
    {
        readonly IEventStore _eventStore;
        readonly IDomainIdentityService _ids;
        public BookingApplicationService(IEventStore eventStore)
        {
            _eventStore = eventStore;
           
      
        }
        
        public void Execute(object o)
        {
            RedirectToWhen.InvokeCommand(this, o);
        }

        void Update(ICommand<BookId> c, Action<BookingAggregate> action)
        {
            var eventStream = _eventStore.LoadEventStream(c.Id);
            var state = new BookingState(eventStream.Events);
            var agg = new BookingAggregate(state);
            action(agg);
            _eventStore.AppendEventsToStream(c.Id, eventStream.StreamVersion, agg.Changes);
           
        }

        public void When(CreateBooking c)
        {
            try
            {

                Update(c, ar => ar.Create(c.Id, c.Rent));
                SaveBooking(c);
            }
            catch (Exception ex)
            { }
        }


        private void SaveBooking(CreateBooking c)
        {
            var providers = CloudStorage.ForDevelopmentStorage();
            var booking = new CloudTable<Rental>(providers.BuildTableStorage(), "booking");
           
            

            booking.Upsert(

                new CloudEntity<Rental>
                {
                    PartitionKey =  c.Id.Id,
                    RowKey =  c.Rent.ModelName,
                    Timestamp = DateTime.UtcNow,
                    Value = c.Rent
                }
            );

      
            var bookingMaster = new CloudTable<CarRentalMaster>(providers.BuildTableStorage(), "CarRentalMaster");

          
            var CarEntity =  bookingMaster.Get(c.Rent.ModelName);
             foreach(var entity in CarEntity)
             {
                 entity.Value.Availabitiy = entity.Value.Availabitiy-1;
                 bookingMaster.Update(entity);
             }
          
         

        }


    }
}
