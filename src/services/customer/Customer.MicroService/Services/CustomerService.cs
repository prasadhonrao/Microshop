﻿using Customer.MicroService.Entities;
using Customer.MicroService.Repositories;
using System.Linq.Expressions;

namespace Customer.MicroService.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository customerRepository;
        

        public CustomerService(
                ICustomerRepository customerRepository)
        {
            this.customerRepository = customerRepository;
            
        }

        public void Add(CustomerEntity entity)
        {
            if (entity  == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
      
            customerRepository.Add( entity );
        }

        public void Update(int id, CustomerEntity entity)
        {
            if (id <= 0)
            {
                throw new ArgumentException(nameof(id));
            }

            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            customerRepository.Update( id, entity );
        }

        public void Patch(int id, CustomerEntity entity)
        {
            if (id <= 0)
            {
                throw new ArgumentException(nameof(id));
            }

            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            customerRepository.Patch(id, entity);
        }

        public async void Delete(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException(nameof(id));
            }

            var entity = await customerRepository.GetAsync(id);
            
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            customerRepository.Delete(entity);
        }

        public Task<IEnumerable<CustomerEntity>> FindAsync(Expression<Func<CustomerEntity, bool>> predicate)
        {
            return customerRepository.FindAsync( predicate );
        }

        public Task<IEnumerable<CustomerEntity>> GetAllAsync()
        {
            return customerRepository.GetAllAsync();
        }

        public Task<CustomerEntity> GetAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException($"Customer Id can not be a negative value");
            }

            return customerRepository.GetAsync( id );
        }

        public Task<bool> SaveChangesAsync()
        {
            return customerRepository.SaveChangesAsync();
        }

        public async Task<bool> CustomerExistsAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException($"Customer Id can not be a negative value");
            }
            return await customerRepository.CustomerExistsAsync(id);
        }

    }
}
