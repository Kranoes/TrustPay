using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using TrustPay.Application.Common.Interfaces;
using TrustPay.Domain.Common;
using TrustPay.Domain.Entities;

namespace TrustPay.Application.Users.Commands.CreateUser
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand,Result<Guid>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        public CreateUserCommandHandler(IUserRepository userRepository,IUnitOfWork unitOfWork) 
        {
        
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        }
        public async Task<Result<Guid>> Handle(CreateUserCommand command, CancellationToken cancellationToken)
        {
            var userResult = User.Create(command.Email,command.NickName);
            if (userResult.IsFailure)
            {
                return Result.Failure <Guid>(userResult.Error);
            }
            var user = userResult.Value;
            var checkEmail = await _userRepository.IsEmailUnique(command.Email, cancellationToken);
            if (!checkEmail)
            {
                return Result.Failure<Guid>("Ошибка создания.Пользователь с таким email уже существует. ");
            }
            var checkNickName = await _userRepository.IsNickNameUnique(command.NickName, cancellationToken);
            if (!checkNickName)
            {
                return Result.Failure<Guid>("Ошибка создания.Пользователь с таким ником уже существует.");
            }
            await _userRepository.AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success(user.Id);


        }
    }
}
