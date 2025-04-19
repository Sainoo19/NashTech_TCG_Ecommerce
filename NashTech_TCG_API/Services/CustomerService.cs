using Microsoft.AspNetCore.Identity;
using NashTech_TCG_API.Models.DTOs;
using NashTech_TCG_API.Models;
using NashTech_TCG_API.Repositories.Interfaces;
using NashTech_TCG_API.Services.Interfaces;

namespace NashTech_TCG_API.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(
            ICustomerRepository customerRepository,
            UserManager<ApplicationUser> userManager,
            ILogger<CustomerService> logger)
        {
            _customerRepository = customerRepository;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<CustomerDTO> GetByIdAsync(string id)
        {
            var user = await _customerRepository.GetByIdAsync(id);

            if (user == null || !await _customerRepository.IsInRoleAsync(user, "Customer"))
            {
                return null;
            }

            return MapToDTO(user);
        }

        public async Task<IEnumerable<CustomerDTO>> GetAllAsync()
        {
            var customers = await _userManager.GetUsersInRoleAsync("Customer");
            return customers.Select(c => MapToDTO(c));
        }

        public async Task<(IEnumerable<CustomerDTO> Customers, int TotalCount, int TotalPages)> GetPagedCustomersAsync(
            int pageNumber,
            int pageSize,
            string searchTerm = null,
            string sortBy = null,
            bool ascending = true)
        {
            var (customers, totalCount) = await _customerRepository.GetPagedCustomersAsync(
                pageNumber, pageSize, searchTerm, sortBy, ascending);

            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var customerDTOs = customers.Select(c => MapToDTO(c));

            return (customerDTOs, totalCount, totalPages);
        }

        public async Task<CustomerDTO> CreateCustomerAsync(CreateCustomerDTO customerDTO)
        {
            try
            {
                var user = new ApplicationUser
                {
                    UserName = customerDTO.UserName,
                    Email = customerDTO.Email,
                    FirstName = customerDTO.FirstName,
                    LastName = customerDTO.LastName,
                    PhoneNumber = customerDTO.PhoneNumber
                };

                var result = await _userManager.CreateAsync(user, customerDTO.Password);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to create customer: {Errors}", errors);
                    return null;
                }

                await _userManager.AddToRoleAsync(user, "Customer");

                _logger.LogInformation($"Created new customer with ID: {user.Id}");
                return MapToDTO(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer");
                throw;
            }
        }

        public async Task<CustomerDTO> UpdateCustomerAsync(string id, UpdateCustomerDTO customerDTO)
        {
            var user = await _customerRepository.GetByIdAsync(id);

            if (user == null || !await _customerRepository.IsInRoleAsync(user, "Customer"))
            {
                _logger.LogWarning($"Customer not found: {id}");
                return null;
            }

            user.FirstName = customerDTO.FirstName;
            user.LastName = customerDTO.LastName;
            user.PhoneNumber = customerDTO.PhoneNumber;

            var result = await _customerRepository.UpdateCustomerAsync(user);

            if (!result)
            {
                _logger.LogError($"Failed to update customer: {id}");
                return null;
            }

            await _customerRepository.SaveChangesAsync();

            _logger.LogInformation($"Updated customer: {id}");
            return MapToDTO(user);
        }

        public async Task<bool> DeleteCustomerAsync(string id)
        {
            var user = await _customerRepository.GetByIdAsync(id);

            if (user == null || !await _customerRepository.IsInRoleAsync(user, "Customer"))
            {
                _logger.LogWarning($"Customer not found: {id}");
                return false;
            }

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                _logger.LogError($"Failed to delete customer: {id}");
                return false;
            }

            _logger.LogInformation($"Deleted customer: {id}");
            return true;
        }

        private CustomerDTO MapToDTO(ApplicationUser user)
        {
            return new CustomerDTO
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber
            };
        }
    }
}
