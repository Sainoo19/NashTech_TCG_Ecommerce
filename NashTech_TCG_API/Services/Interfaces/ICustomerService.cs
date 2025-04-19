using NashTech_TCG_API.Models.DTOs;

namespace NashTech_TCG_API.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<CustomerDTO> GetByIdAsync(string id);
        Task<IEnumerable<CustomerDTO>> GetAllAsync();
        Task<(IEnumerable<CustomerDTO> Customers, int TotalCount, int TotalPages)> GetPagedCustomersAsync(
            int pageNumber,
            int pageSize,
            string searchTerm = null,
            string sortBy = null,
            bool ascending = true);
        Task<CustomerDTO> CreateCustomerAsync(CreateCustomerDTO customerDTO);
        Task<CustomerDTO> UpdateCustomerAsync(string id, UpdateCustomerDTO customerDTO);
        Task<bool> DeleteCustomerAsync(string id);
    }
}
