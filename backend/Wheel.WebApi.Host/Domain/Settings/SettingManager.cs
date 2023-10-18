﻿using HotChocolate.Types.Relay;
using System.Linq;
using Wheel.DependencyInjection;
using Wheel.Enums;
using Wheel.Uow;
using Wheel.Utilities;

namespace Wheel.Domain.Settings
{
    public class SettingManager : ITransientDependency
    {
        private readonly IBasicRepository<SettingGroup, long> _settingGroupRepository;
        private readonly IBasicRepository<SettingValue, long> _settingValueRepository;

        private readonly IUnitOfWork _unitOfWork;

        public SettingManager(IBasicRepository<SettingGroup, long> settingGroupRepository, IBasicRepository<SettingValue, long> settingValueRepository, IUnitOfWork unitOfWork)
        {
            _settingGroupRepository = settingGroupRepository;
            _settingValueRepository = settingValueRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<T?> GetSettingValue<T>(string settingGroupName, string settingKey, SettingScope settingScope = SettingScope.Golbal, string? settingScopeKey = null, CancellationToken cancellationToken = default)
        {
            var settingGroup = await _settingGroupRepository.FindAsync(a => a.Name == settingGroupName, cancellationToken);

            if (settingGroup is null)
            {
                throw new ArgumentException($"SettingGroup: {settingGroup} Not Exist.");
            }

            var settingValue = settingGroup?.SettingValues.FirstOrDefault(a => a.Key == settingKey && a.SettingScope == settingScope && a.SettingScopeKey == settingScopeKey);

            if (settingValue is null)
                return default;

            if(settingValue.ValueType == SettingValueType.JsonObject)
                return System.Text.Json.JsonSerializer.Deserialize<T>(settingValue.Value);

            return (T)Convert.ChangeType(settingValue, typeof(T));
        }

        public async Task<SettingValue?> GetSettingValue(string settingGroupName, string settingKey, SettingScope settingScope = SettingScope.Golbal, string? settingScopeKey = null, CancellationToken cancellationToken = default)
        {
            var settingGroup = await _settingGroupRepository.FindAsync(a => a.Name == settingGroupName, cancellationToken);

            if (settingGroup is null)
            {
                throw new ArgumentException($"SettingGroup: {settingGroup} Not Exist.");
            }

            var settingValue = settingGroup?.SettingValues.FirstOrDefault(a => a.Key == settingKey && a.SettingScope == settingScope && a.SettingScopeKey == settingScopeKey);
            
            return settingValue;
        }

        public async Task<List<SettingValue>?> GetSettingValues(string settingGroupName, SettingScope settingScope = SettingScope.Golbal, string? settingScopeKey = null, CancellationToken cancellationToken = default)
        {
            var settingGroup = await _settingGroupRepository.FindAsync(a => a.Name == settingGroupName, cancellationToken);

            if (settingGroup is null)
            {
                throw new ArgumentException($"SettingGroup: {settingGroup} Not Exist.");
            }

            var settingValues = settingGroup?.SettingValues.Where(a => a.SettingScope == settingScope && a.SettingScopeKey == settingScopeKey).ToList();

            return settingValues;
        }

        public async Task SetSettingValue(string settingGroupName, SettingValue settingValue, CancellationToken cancellationToken = default)
        {
            using (var uow = await _unitOfWork.BeginTransactionAsync(cancellationToken))
            {
                try
                {
                    var settingGroup = await _settingGroupRepository.FindAsync(a => a.Name == settingGroupName, cancellationToken);
                    if (settingGroup is null)
                        settingGroup = await _settingGroupRepository.InsertAsync(new SettingGroup { Name = settingGroupName, NormalizedName = settingGroupName.ToUpper() }, cancellationToken: cancellationToken);
                    

                    CheckSettingValueType(settingValue.Value, settingValue.ValueType);

                    var sv = await _settingValueRepository.FindAsync(a=> a.SettingGroupId == settingGroup.Id && a.Id == settingValue.Id, cancellationToken);
                    if(sv is null)
                        await _settingValueRepository.InsertAsync(settingValue, cancellationToken: cancellationToken);
                    else
                        await _settingValueRepository.UpdateAsync(settingValue, cancellationToken: cancellationToken);
                    
                    await uow.CommitAsync(cancellationToken);
                }catch(Exception ex)
                {
                    await uow.RollbackAsync(cancellationToken);
                    throw ex;
                }
            }
        }
        public async Task SetSettingValues(string settingGroupName, List<SettingValue> settingValues, CancellationToken cancellationToken = default)
        {
            using (var uow = await _unitOfWork.BeginTransactionAsync(cancellationToken))
            {
                try
                {
                    var settingGroup = await _settingGroupRepository.FindAsync(a => a.Name == settingGroupName, cancellationToken);
                    if (settingGroup is null)
                        settingGroup = await _settingGroupRepository.InsertAsync(new SettingGroup { Name = settingGroupName, NormalizedName = settingGroupName.ToUpper() }, cancellationToken: cancellationToken);

                    foreach (var settingValue in settingValues)
                    {
                        CheckSettingValueType(settingValue.Value, settingValue.ValueType);

                        var sv = await _settingValueRepository.FindAsync(a => a.SettingGroupId == settingGroup.Id && a.Id == settingValue.Id, cancellationToken);
                        if (sv is null)
                            await _settingValueRepository.InsertAsync(settingValue, cancellationToken: cancellationToken);
                        else
                            await _settingValueRepository.UpdateAsync(settingValue, cancellationToken: cancellationToken);
                    }
                    
                    await uow.CommitAsync(cancellationToken);
                }catch(Exception ex)
                {
                    await uow.RollbackAsync(cancellationToken);
                    throw ex;
                }
            }
        }

        private void CheckSettingValueType(string settingValue, SettingValueType settingValueType)
        {
            switch (settingValueType)
            {
                case SettingValueType.String:
                case SettingValueType.JsonObject:
                    return;
                case SettingValueType.Bool:
                    if(bool.TryParse(settingValue, out var _))
                    {
                        return;
                    }
                    else
                    {
                        throw new ArgumentException($"SettingValue: {settingValue} Can Not Parse To Bool Type");
                    }
                case SettingValueType.Int:
                    if (int.TryParse(settingValue, out var _))
                    {
                        return;
                    }
                    else
                    {
                        throw new ArgumentException($"SettingValue: {settingValue} Can Not Parse To Int Type");
                    }
                case SettingValueType.Long:
                    if (long.TryParse(settingValue, out var _))
                    {
                        return;
                    }
                    else
                    {
                        throw new ArgumentException($"SettingValue: {settingValue} Can Not Parse To Long Type");
                    }
                case SettingValueType.Double:
                    if (double.TryParse(settingValue, out var _))
                    {
                        return;
                    }
                    else
                    {
                        throw new ArgumentException($"SettingValue: {settingValue} Can Not Parse To Double Type");
                    }
                case SettingValueType.Decimal:
                    if (decimal.TryParse(settingValue, out var _))
                    {
                        return;
                    }
                    else
                    {
                        throw new ArgumentException($"SettingValue: {settingValue} Can Not Parse To Decimal Type");
                    }
            }
        }
    }
}