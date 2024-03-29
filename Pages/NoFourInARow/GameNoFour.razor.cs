﻿using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Portfolio.Interfaces;
using Portfolio.Models;
using Portfolio.Models.Aws;
using Portfolio.Services;
using System.Net.Http.Json;
using System.Timers;

namespace Portfolio.Pages.NoFourInARow
{
    public partial class GameNoFour : ComponentBase
    {
        [Parameter]
        public int DifficultyParameter { get; set; }

        [Parameter]
        public int Index { get; set; }

        [Parameter]
        public CellModel[,] Cells { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        [Inject]
        public ILeaderboardNoFourService LeaderboardNoFourService { get; set; }

        Difficulty Difficulty;
        FileConfigNoFourInARow fileConfig;
        GameData game;
        Modal modalWin;

        string timeText = "00:00:00";
        System.Timers.Timer timer;
        DateTime startTime = DateTime.Now;
        TimeSpan finish;
        bool hasError;

        ActionStatusCell _ActionStatusCellRequestedBeforeClick;

        private async Task<Task> ShowModal()
        {
            timer.Stop();
            GridsDoneStorage gridsStorage = await _localstorage.GetItemAsync<GridsDoneStorage>($"grid{Difficulty.GetDifficultyPath()}");

            if (gridsStorage == null)
                gridsStorage = new GridsDoneStorage();

            var grid = gridsStorage.Grids.FirstOrDefault(x => x.Index == Index);
            int time = finish.Minutes * 60 + finish.Seconds;
            if (grid == null)
                grid = new();
            else if (grid.Seconds < time)
                return modalWin.Show();

            grid.Index = Index;
            grid.Seconds = time;
            gridsStorage.Grids.Add(grid);

            Task taskScore;
            var user = (await authenticationStateTask).User;
            if (!user.Identity.IsAuthenticated)
            {
                await _localstorage.SetItemAsync<GridsDoneStorage>($"grid{Difficulty.GetDifficultyPath()}", gridsStorage);
                await modalWin.Show();
            }
            else
            {
                taskScore = SendScore(grid);
                await modalWin.Show();
                await taskScore;
            }


            return Task.CompletedTask;
        }

        async Task SendScore(GridDoneStorage grid)
        {
            var user = (await authenticationStateTask).User;
            if (!user.Identity.IsAuthenticated)
                return;

            string username = user.Claims.FirstOrDefault(x => x.Type == "username")?.Value;
            LeaderboardSaveRequest contentRequest = new()
            {
                Difficulty = (int)Difficulty,
                Level = grid.Index,
                Name = username,
                Seconds = grid.Seconds
            };

            try
            {
                var response = await LeaderboardNoFourService.PostAsJsonAsync("/leaderboardsave", contentRequest);
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync("Un problème dans l'envoi du score");
            }
        }

        void ResetErrorGrid()
        {
            for (int y = 0; y < fileConfig.Height; y += 1)
            {
                for (int x = 0; x < fileConfig.Width; x += 1)
                {
                    Cells[y, x].IsInError = false;
                }
            }
        }

        bool CheckDiagonalLeft(int y, int x)
        {
            if (y - 3 < 0 || x + 3 > fileConfig.Width || Cells == null)
                return false;

            CellModel startCell = Cells[y, x];
            StatusCell startStatusCell = startCell.Status;
            for (int yParcours = y - 1, xParcours = x + 1; yParcours > y - 4 && xParcours < x + 4; yParcours -= 1, xParcours += 1)
            {
                if (yParcours < 0 || xParcours >= fileConfig.Width || Cells[yParcours, xParcours].Status != startStatusCell)
                    return false;
            }

            for (int yParcours = y, xParcours = x; yParcours > y - 4 && xParcours < x + 4; yParcours -= 1, xParcours += 1)
                Cells[yParcours, xParcours].IsInError = true;

            return true;
        }

        bool CheckDiagonalRight(int y, int x)
        {
            if (y + 3 > fileConfig.Height || x + 3 > fileConfig.Width || Cells == null)
                return false;

            CellModel startCell = Cells[y, x];
            StatusCell startStatusCell = startCell.Status;
            for (int yParcours = y + 1, xParcours = x + 1; yParcours < y + 4 && xParcours < x + 4; yParcours += 1, xParcours += 1)
            {
                if (yParcours >= fileConfig.Height || xParcours >= fileConfig.Width || Cells[yParcours, xParcours].Status != startStatusCell)
                    return false;
            }

            for (int yParcours = y, xParcours = x; yParcours < y + 4 && xParcours < x + 4; yParcours += 1, xParcours += 1)
                Cells[yParcours, xParcours].IsInError = true;

            return true;
        }

        bool CheckHorizontal(int y, int x)
        {
            if (y + 3 > fileConfig.Height || Cells == null)
                return false;

            CellModel startCell = Cells[y, x];
            StatusCell startStatusCell = startCell.Status;
            for (int yParcours = y + 1; yParcours < y + 4; yParcours += 1)
            {
                if (yParcours >= fileConfig.Height || Cells[yParcours, x].Status != startStatusCell)
                    return false;
            }

            for (int yParcours = y; yParcours < y + 4; yParcours += 1)
                Cells[yParcours, x].IsInError = true;

            return true;
        }

        bool CheckVertical(int y, int x)
        {
            if (x + 3 > fileConfig.Width || Cells == null)
                return false;

            CellModel startCell = Cells[y, x];
            StatusCell startStatusCell = startCell.Status;
            for (int xParcours = x + 1; xParcours < x + 4; xParcours += 1)
            {
                if (xParcours >= fileConfig.Width || Cells[y, xParcours].Status != startStatusCell)
                    return false;
            }

            for (int xParcours = x; xParcours < x + 4; xParcours += 1)
                Cells[y, xParcours].IsInError = true;

            return true;
        }

        bool CheckGridInError()
        {
            ResetErrorGrid();
            hasError = false;
            for (int y = 0; y < fileConfig.Height; y += 1)
            {
                for (int x = 0; x < fileConfig.Width; x += 1)
                {
                    if (Cells[y, x].Status == StatusCell.Empty)
                        continue;
                    hasError |= CheckVertical(y, x);
                    hasError |= CheckHorizontal(y, x);
                    hasError |= CheckDiagonalRight(y, x);
                    hasError |= CheckDiagonalLeft(y, x);
                }
            }

            return hasError;
        }

        bool CheckWin()
        {
            for (int y = 0; y < fileConfig.Height; y += 1)
            {
                for (int x = 0; x < fileConfig.Width; x += 1)
                {
                    if (Cells[y, x].Status == StatusCell.Empty)
                        return false;
                }
            }

            return true;
        }

        Task OnClickedEraseCell(int x, int y)
        {
            CellModel cell = Cells[y, x];
            if (cell.IsBase)
                return Task.CompletedTask;

            cell.Status = StatusCell.Empty;


            CheckGridInError();
            return Task.CompletedTask;
        }


        Task OnMiddleClickedCell(int x, int y)
        {
            CellModel cell = Cells[y, x];
            if (cell.IsBase || cell.Status == StatusCell.Circle)
                return Task.CompletedTask;

            cell.Status = StatusCell.Circle;

            bool hasError = CheckGridInError();
            if (!hasError && CheckWin())
                ShowModal();

            return Task.CompletedTask;
        }

        Task OnClickedCell(int x, int y)
        {
            CellModel cell = Cells[y, x];
            if (cell.IsBase)
                return Task.CompletedTask;

            if (_ActionStatusCellRequestedBeforeClick != ActionStatusCell.Nothing)
            {

                // Si on est en mobile, on a pas click droit, donc on va toujours passer ici
                switch (_ActionStatusCellRequestedBeforeClick)
                {
                    case ActionStatusCell.Cross:
                        cell.Status = StatusCell.Cross;
                        break;
                    case ActionStatusCell.Circle:
                        cell.Status = StatusCell.Circle;
                        break;
                    case ActionStatusCell.Erease:
                        cell.Status = StatusCell.Empty;
                        break;
                }
            }
            else
            {
                if (cell.Status == StatusCell.Cross)
                    return Task.CompletedTask;

                cell.Status = StatusCell.Cross;
            }

            bool hasError = CheckGridInError();
            if (!hasError && CheckWin())
                ShowModal();

            return Task.CompletedTask;
        }

        Task OnClickedCellFromSelectWhichBefore(ActionStatusCell actionStatusRequested)
        {
            _ActionStatusCellRequestedBeforeClick = actionStatusRequested;
            return Task.CompletedTask;
        }

        protected override void OnParametersSet()
        {
            Difficulty = (Difficulty)DifficultyParameter;
        }

        void GetBack()
        {
            UriHelper.NavigateTo($"/nofour/level/{(int)Difficulty}");
        }

        void NextLevel()
        {
            UriHelper.NavigateTo($"/nofour/level/{(int)Difficulty}");

            game = fileConfig.Grids.FirstOrDefault(x => x.Index == Index + 1);
            if (game != null)
            {
                UriHelper.NavigateTo($"/nofour/game/{(int)Difficulty}/{Index + 1}");
            }
        }

        void ResetGrid()
        {
            for (int y = 0; y < fileConfig.Height; y += 1)
            {
                var line = game.Grid.Skip(y * fileConfig.Width).Take(fileConfig.Width);
                int x = 0;
                foreach (var item in line)
                {
                    CellModel cell = new CellModel() { Status = (StatusCell)item };
                    if (cell.Status != StatusCell.Empty)
                        cell.IsBase = true;
                    Cells[y, x] = cell;
                    x += 1;
                }
            }
            startTime = DateTime.Now;
        }

        void OnTimedEvent(object? sender, ElapsedEventArgs e)
        {
            DateTime currentTime = e.SignalTime;
            finish = currentTime.Subtract(startTime);
            timeText = $"{finish.Minutes:00}:{finish.Seconds:00}.{finish.Milliseconds:000}";
            StateHasChanged();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await base.OnInitializedAsync();
                string resFile = $"nofourinarow/{Difficulty.GetDifficultyPath()}/grids.json";
                fileConfig = await Http.GetFromJsonAsync<FileConfigNoFourInARow>(resFile);
                game = fileConfig.Grids.FirstOrDefault(x => x.Index == Index);

                if (game == null)
                {
                    UriHelper.NavigateTo($"/nofour/level/{(int)Difficulty}");
                }

                Cells = new CellModel[fileConfig.Height, fileConfig.Width];
                for (var i = 0; i < fileConfig.Height * fileConfig.Width; i++)
                {
                    Cells[i / fileConfig.Width, i % fileConfig.Width] = new CellModel();
                }

                ResetGrid();

                timer = new System.Timers.Timer(1);
                timer.Elapsed += OnTimedEvent;
                timer.AutoReset = true;
                timer.Enabled = true;
                StateHasChanged();
            }
        }
    }
}
