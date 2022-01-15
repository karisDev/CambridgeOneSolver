﻿using Squirrel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CambridgeOneSolver.Models
{
    class SquirrelUpdateService
    {
        private Action<string, bool> ReturnStatus;
        // этот код нужно переделать
        public async void CheckForUpdates()
        {
            ReturnStatus("Проверяем обновления", true);
            try
            {
                using (UpdateManager manager = await UpdateManager
                    .GitHubUpdateManager(@"https://github.com/karisDev/cos-updates"))
                {
                    SquirrelAwareApp.HandleEvents(
                        onInitialInstall: v =>
                        {
                            manager.CreateShortcutForThisExe();
                            Application.Current.Shutdown();
                        });
                    ReturnStatus("Загружаем новую версию", true);

                    var UpdateInfo = await manager.CheckForUpdate();
                    if (UpdateInfo.ReleasesToApply.Count > 0)
                    {
                        try
                        {
                            await manager.UpdateApp();
                            ReturnStatus("Новая версия готова к установке. Перезапустите приложение", false);
                        }
                        catch
                        {
                            ReturnStatus("Ошибка при установке новой версии", false);
                        }
                    }
                    else
                    {
                        ReturnStatus("Установлена актуальная версия", false);
                    }
                }
            }
            catch
            {
                ReturnStatus("Не удалось установить связь с GitHub", false);
            }

        }
        public SquirrelUpdateService(Action<string, bool> action)
        {
            ReturnStatus = action;
            CheckForUpdates();
        }
    }
}
