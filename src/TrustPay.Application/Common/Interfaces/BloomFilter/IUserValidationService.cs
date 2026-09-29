using System;
using System.Collections.Generic;
using System.Text;

namespace TrustPay.Application.Common.Interfaces.BloomFilter
{
    public interface IUserValidationService
    {
        Task<bool> IsEmailTakenAsync(string email,CancellationToken cancellationToken = default);
        Task RegisterUserEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> IsNickNameTakenAsync(string nickName, CancellationToken cancellationToken = default);
        Task RegisterUserNickNameAsync(string email, CancellationToken cancellationToken = default);


    }
}
