﻿using Wheel.Administrator.Services.FileStorageManage.Dtos;
using Wheel.Core.Dto;
using Wheel.DependencyInjection;
using Wheel.Services.FileStorageManage.Dtos;

namespace Wheel.Administrator.Services.FileStorageManage
{
    public interface IFileStorageManageAppService : ITransientDependency
    {
        Task<Page<FileStorageDto>> GetFileStoragePageList(FileStoragePageRequest request);
        Task<R<List<FileStorageDto>>> UploadFiles(UploadFileDto uploadFileDto);
        Task<R<DownloadFileResonse>> DownloadFile(long id);
    }
}
