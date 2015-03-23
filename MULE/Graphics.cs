using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SdlDotNet;
using SdlDotNet.Core;
using SdlDotNet.Graphics;
using SdlDotNet.Graphics.Primitives;
using SdlDotNet.Graphics.Sprites;
using SdlDotNet.Input;
using SdlDotNet.Windows;
using System.Drawing;
using MULE.States;
using MULE.Sprites;

namespace MULE
{
    public enum ShipType
    {
        Mothership = 0,
        PirateShip = 1
    }

    public class Graphics
    {
        private enum PlayerColors
        {
            Green = 0x83D551,
            Purple = 0xAE59DC,
            Orange = 0xB5772F,
            Blue = 0x4E3DCC,
            Red = 0xFF0000,
            Yellow = 0xA3BB00,
            Brown = 0x6F5A2C,
            Gray = 0x7D7D7D
        }

        private const int BUFFERWIDTH = 1024;
        private const int BUFFERHEIGHT = 768;

        public const int TIMERHEIGHT = 475;
        public const int TIMERWIDTH = 10;

        public const int HORIZONTALBUFFER = 50;
        public const int VERTICALBUFFER = 100;

        public const int PLOTSIZE = 100;
        public const int MOUNTAINWIDTH = 46;
        public const int MOUNTAINHEIGHT = 15;
        public const int MOUNTAINBUFFER = 5;
        public const int HOUSEXOFFSET = 75;
        public const int HOUSEYOFFSET = 25;
        public const int HOUSEWIDTH = 25;
        public const int HOUSEHEIGHT = 25;

        public const int MAPSTOREOFFSET = 5;

        public const int SHIPWIDTH = 150;
        public const int SHIPHEIGHT = 63;

        public const int PLAYERWIDTH = 50;
        public const int PLAYERHEIGHT = 60;
        public const int PLAYERBUFFER = 10;
        public const double SMALLPLAYERSCALE = 0.4d;
        public const int SMALLPLAYERWIDTH = 20;
        public const int SMALLPLAYERHEIGHT = 24;
        public const int SMALLPLAYERBUFFER = 4;

        public const int MULEWIDTH = 100;
        public const int MULEHEIGHT = 60;
        public const int MULEBUFFER = 10;
        public const double SMALLMULESCALE = 0.4d;
        public const int SMALLMULEWIDTH = 40;
        public const int SMALLMULEHEIGHT = 24;
        public const int SMALLMULEBUFFER = 4;

        public const int STOREWIDTH = 600;
        public const int STOREHEIGHT = 475;
        public const int STOREBUFFER = 25;
        public const int DEPTWIDTH = 100;
        public const int DEPTTOPHEIGHT = 100;
        public const int DEPTBOTTOMHEIGHT = 225;
        public const int DEPTMULEWIDTH = 150;
        public const int DEPTMULEHEIGHT = 225;
        public const int DEPTBUFFER = 5;
        public const int DEPTSPACING = 50;
        public const int DEPTTIGHTSPACING = 25;
        public const int MULEICONWIDTH = 50;
        public const int MULEICONHEIGHT = 25;
        public const int MULEICONHBUFFER = 25;
        public const int MULEICONVBUFFER = 28;

        private int numTilesWide;
        private int numTilesHigh;

        private Game game;

        private Surface screen;
        private Surface buffer;
        private Surface map;
        private Surface mapBuffer;
        private Surface storeBuffer;

        private Surface store;
        private Surface plain;
        private Surface river;
        private Surface mountain;
        private Surface plots;
        private Surface ships;
        private Surface cursor;

        private Surface storebg;
        private Surface departments;

        private Surface[] chars;
        private Surface[] charsSmall;

        private Surface mule;
        private Surface muleSmall;

        private TextSprite[] textSprites;

        private double mapScale;
        private double bufferScale;

        private bool fullscreen;
        private bool antiAlias;
        private bool hardware;
        private bool isDrawing;
        private bool stopDraw;

        public Graphics(Game game)
        {
            this.game = game;

            fullscreen = true;
            antiAlias = false;
            hardware = false;

            isDrawing = false;
            stopDraw = false;

            Video.Initialize();
            Video.WindowCaption = "M.U.L.E. Extended Edition";

            createTextSprites();

            //SetMode(1024, 768, fullscreen);
            SetMode(800, 600, fullscreen);

            loadSurfaces();

            Events.TargetFps = 30;
            Events.Tick += new EventHandler<TickEventArgs>(Events_Tick);
        }

        public void Dispose()
        {
            stopDraw = true;
            killSurfaces();
        }

        private void createTextSprites()
        {
            textSprites = new TextSprite[7];

            for (int i = 0; i < textSprites.Length; i++)
                textSprites[i] = new TextSprite(
                    new SdlDotNet.Graphics.Font(
                        game.AppPath + "gfx" + Path.DirectorySeparatorChar + "font.ttf",
                        18 + i * 2
                        ));
        }

        private TextSprite getTextSprite(string text, int size, Color color)
        {
            TextSprite ret = textSprites[(size - 18) / 2];
            ret.Text = text;
            ret.Color = color;

            return ret;
        }

        private Color getPlayerColor(PlayerColor color)
        {
            return
                Color.FromArgb((int)Enum.Parse(typeof(PlayerColors), color.ToString()));            
        }

        private void killSurfaces()
        {
            if (store != null) destroySurface(store);
            if (cursor != null) destroySurface(cursor);
            if (ships != null) destroySurface(ships);
            if (plain != null) destroySurface(plain);
            if (river != null) destroySurface(river);
            if (mountain != null) destroySurface(mountain);
            if (plots != null) destroySurface(plots);
            if (storebg != null) destroySurface(storebg);
            if (departments != null) destroySurface(departments);

            if (map != null) destroySurface(map);
            if (mapBuffer != null) destroySurface(mapBuffer);
            if (storeBuffer != null) destroySurface(storeBuffer);
            if (buffer != null) destroySurface(buffer);

            if (chars != null)
            {
                for (int i = 0; i < chars.Length; i++)
                    destroySurface(chars[i]);
            }

            if (charsSmall != null)
                for (int i = 0; i < charsSmall.Length; i++)
                    destroySurface(charsSmall[i]);

            if (mule != null) destroySurface(mule);
            if (muleSmall != null) destroySurface(muleSmall);
        }

        private void loadSurfaces()
        {
            killSurfaces();

            store =
                new Surface(
                    game.AppPath + "gfx" + 
                    Path.DirectorySeparatorChar + 
                    "store.png"
                    ).Convert(screen, hardware, false);
            store.Transparent = true;
            store.TransparentColor = Color.FromArgb(255, 0, 255);

            cursor =
                new Surface(
                    game.AppPath + "gfx" +
                    Path.DirectorySeparatorChar +
                    "cursor.png"
                    ).Convert(screen, hardware, false);
            cursor.Transparent = true;
            cursor.TransparentColor = Color.FromArgb(255, 0, 255);

            ships =
                new Surface(
                    game.AppPath + "gfx" +
                    Path.DirectorySeparatorChar +
                    "ships.png"
                    ).Convert(screen, hardware, false);
            ships.Transparent = true;
            ships.TransparentColor = Color.FromArgb(255, 0, 255);

            plain =
                new Surface(
                    game.AppPath + "gfx" +
                    Path.DirectorySeparatorChar +
                    "plain.png"
                    ).Convert(screen, hardware, false);
            plain.Transparent = true;
            plain.TransparentColor = Color.FromArgb(255, 0, 255);

            river =
                new Surface(
                    game.AppPath + "gfx" + 
                    Path.DirectorySeparatorChar +
                    "river.png"
                    ).Convert(screen, hardware, false);
            river.Transparent = true;
            river.TransparentColor = Color.FromArgb(255, 0, 255);

            mountain =
                new Surface(
                    game.AppPath + "gfx" +
                    Path.DirectorySeparatorChar +
                    "mountain.png"
                    ).Convert(screen, hardware, false);
            mountain.Transparent = true;
            mountain.TransparentColor = Color.FromArgb(255, 0, 255);

            plots =
                new Surface(
                    game.AppPath + "gfx" +
                    Path.DirectorySeparatorChar +
                    "plots.png"
                    ).Convert(screen, hardware, false);
            plots.Transparent = true;
            plots.TransparentColor = Color.FromArgb(255, 0, 255);

            storebg =
                new Surface(
                    game.AppPath + "gfx" +
                    Path.DirectorySeparatorChar +
                    "storebg.png"
                    ).Convert(screen, hardware, false);
            storebg.Transparent = true;
            storebg.TransparentColor = Color.FromArgb(255, 0, 255);

            storeBuffer = storebg.CreateCompatibleSurface();

            departments =
                new Surface(
                    game.AppPath + "gfx" +
                    Path.DirectorySeparatorChar +
                    "departments.png"
                    ).Convert(screen, hardware, false);
            departments.Transparent = true;
            departments.TransparentColor = Color.FromArgb(255, 0, 255);
            
            chars = new Surface[8];
            charsSmall = new Surface[8];

            for (int i = 0; i < 1; i++)
            {
                chars[i] =
                    new Surface(
                        game.AppPath + "gfx" +
                        Path.DirectorySeparatorChar +
                        ((PlayerType)(i + 1)).ToString().ToLower() + ".png"
                        ).Convert(screen, hardware, false);
                chars[i].Transparent = true;
                chars[i].TransparentColor = Color.FromArgb(255, 0, 255);

                charsSmall[i] =
                    chars[i].CreateScaledSurface(SMALLPLAYERSCALE, antiAlias);
                charsSmall[i].Transparent = true;
                charsSmall[i].TransparentColor = Color.FromArgb(255, 0, 255);
            }

            mule =
                new Surface(
                    game.AppPath + "gfx" +
                    Path.DirectorySeparatorChar +
                    "mule.png"
                    ).Convert(screen, hardware, false);
            mule.Transparent = true;
            mule.TransparentColor = Color.FromArgb(255, 0, 255);

            muleSmall =
                mule.CreateScaledSurface(SMALLMULESCALE, antiAlias);
            muleSmall.Transparent = true;
            muleSmall.TransparentColor = Color.FromArgb(255, 0, 255);
        }

        public int Width
        {
            get { return buffer.Width; }
        }

        public int Height
        {
            get { return buffer.Height; }
        }

        public double MapScale
        {
            get { return mapScale; }
        }

        public bool Drawing
        {
            get { return !stopDraw; }
            set { stopDraw = !value; }
        }

        public void SetMode(int width, int height, bool fullscreen)
        {
            stopDraw = true;

            this.fullscreen = fullscreen;

            Surface oldScreen = null;

            if (screen != null)
                oldScreen = screen;

            Mouse.ShowCursor = !fullscreen;

            screen =
                Video.SetVideoMode(
                    width,
                    height,
                    false,
                    false, 
                    fullscreen, 
                    hardware
                    );

            if (oldScreen != null)
                destroySurface(oldScreen);

            getScales();

            stopDraw = false;
        }

        public void SetTiles(int width, int height)
        {
            int bufferWidth = BUFFERWIDTH;

            if ((double)screen.Width / (double)screen.Height >
                (double)BUFFERWIDTH / (double)BUFFERHEIGHT)
                bufferWidth = 
                    (int)(
                        (double)BUFFERHEIGHT * 
                        ((double)screen.Width / (double)screen.Height)
                        );

            Surface oldBuffer = null;

            if (buffer != null)
                oldBuffer = buffer;

            buffer =
                screen.CreateCompatibleSurface(bufferWidth, BUFFERHEIGHT);

            if (oldBuffer != null)
                destroySurface(oldBuffer);

            numTilesWide = width;
            numTilesHigh = height;

            createMap();

            getScales();
        }

        private void getScales()
        {
            if (buffer != null)
            {
                bufferScale = 1d;

                if ((double)buffer.Width / (double)screen.Width >
                    (double)buffer.Height / (double)screen.Height)
                    bufferScale = (double)screen.Width / (double)buffer.Width;
                else
                    bufferScale = (double)screen.Height / (double)buffer.Height;
            }

            if (mapBuffer != null)
            {
                mapScale = 1d;

                if ((double)mapBuffer.Width / (double)(buffer.Width - HORIZONTALBUFFER * 2) >
                    (double)mapBuffer.Height / (double)(buffer.Height - VERTICALBUFFER * 2))
                    mapScale = (double)(buffer.Width - HORIZONTALBUFFER * 2) / (double)mapBuffer.Width;
                else
                    mapScale = (double)(buffer.Height - VERTICALBUFFER * 2) / (double)mapBuffer.Height;
            }
        }

        private void createMap()
        {
            Surface oldMap = null;

            if (map != null)
                oldMap = map;

            map = screen.CreateCompatibleSurface(
                numTilesWide * PLOTSIZE,
                numTilesHigh * PLOTSIZE
                );

            if (oldMap != null)
                destroySurface(oldMap);

            map.Fill(Color.White);

            for (int y = 0; y < numTilesHigh; y++)
                for (int x = 0; x < numTilesWide; x++)
                {
                    map.Blit(
                        plain,
                        new Point(x * PLOTSIZE, y * PLOTSIZE)
                        );

                    switch (game.Map.Plots[x, y].Type)
                    {
                        case PlotType.Store:
                            map.Blit(
                                store,
                                new Point(x * PLOTSIZE, y * PLOTSIZE)
                                );
                            break;

                        case PlotType.River:
                            map.Blit(
                                river,
                                new Point(x * PLOTSIZE, y * PLOTSIZE),
                                new Rectangle(
                                    0,
                                    (y > numTilesHigh / 2 ?
                                        (y % 4 == 0 ? 3 : (y % 4) - 1) :
                                        y % 4
                                    ) * PLOTSIZE,
                                    PLOTSIZE,
                                    PLOTSIZE
                                ));
                            break;

                        case PlotType.Mountains:
                            Mountain[] mountains = game.Map.Plots[x, y].Mountains;

                            for (int i = 0; i < mountains.Length; i++)
                            {
                                map.Blit(
                                    mountain,
                                    new Point(
                                        x * PLOTSIZE + mountains[i].X,
                                        y * PLOTSIZE + mountains[i].Y
                                        ),
                                    new Rectangle(
                                        0,
                                        ((int)mountains[i].Type - 1) * MOUNTAINHEIGHT,
                                        MOUNTAINWIDTH,
                                        MOUNTAINHEIGHT
                                        ));
                            }
                            break;
                    }
                }

            Surface oldMapBuffer = null;

            if (mapBuffer != null)
                oldMapBuffer = mapBuffer;

            mapBuffer = screen.CreateCompatibleSurface(
                numTilesWide * PLOTSIZE,
                numTilesHigh * PLOTSIZE
                );

            if (oldMapBuffer != null)
                destroySurface(oldMapBuffer);
        }

        private void destroySurface(Surface surface)
        {
            Tao.Sdl.Sdl.SDL_FreeSurface(surface.Handle);
            surface.Close();
        }

        private void Events_Tick(object sender, TickEventArgs e)
        {
            if (isDrawing || stopDraw) return;

            isDrawing = true;

            if (game.State != null)
            {
                if (game.State is LandGrantState)
                {
                    buffer.Fill(Color.White);

                    drawMap();
                    drawMapHeaderText("LAND GRANT");
                    drawMapFooterText("PRESS YOUR BUTTON TO SELECT A PLOT");
                }
                else if(game.State is TransportState)
                {
                    buffer.Fill(Color.White);

                    drawMap();

                    TransportState state = (TransportState)game.State;

                    if (state.State == TransportState.LocalState.Lifting ||
                        state.State == TransportState.LocalState.Leaving ||
                        state.ShipRectangle.X < 0)
                    {
                        drawMapFooterText(
                            "THE SHIP WILL BE BACK IN " +
                            game.NumRounds.ToString() +
                            " MONTHS."
                            );
                    }
                    else
                    {
                        drawMapHeaderText("TRANSPORT SHIP");
                        drawMapFooterText("YOU'RE LANDING ON THE PLANET IRATA.");
                    }
                }
                else if (game.State is SummaryState)
                {
                    buffer.Fill(Color.Black);
                    drawSummary();
                }
                else if (game.State is PlayerTurnState)
                {
                    buffer.Fill(Color.White);

                    PlayerTurnState state = (PlayerTurnState)game.State;

                    drawMapHeaderText("DEVELOPMENT #" + game.Round.ToString());

                    if (state.State == PlayerTurnState.LocalState.WaitForButton)
                    {
                        drawMap();

                        if (state.State == PlayerTurnState.LocalState.WaitForButton)
                            drawMapFooterText("PRESS YOUR STICK BUTTON TO START");
                    }
                    else if (state.State == PlayerTurnState.LocalState.ZoomInStore ||
                        state.State == PlayerTurnState.LocalState.ZoomOutStore)
                    {
                        Surface scaledStore =
                            storebg.CreateScaledSurface(state.StoreScale);

                        buffer.Blit(
                            scaledStore,
                            new Point(
                                buffer.Width / 2 - scaledStore.Width / 2,
                                buffer.Height / 2 - scaledStore.Height / 2
                                ));

                        destroySurface(scaledStore);

                        drawTicker(state.Ticker.Value);
                    }
                    else if (state.State == PlayerTurnState.LocalState.InStore ||
                        state.State == PlayerTurnState.LocalState.Pub)
                    {
                        drawStore(
                            state.PlayerNum, 
                            state.PlayerSprite,
                            state.MuleSprite, 
                            state.HasMule);
                        drawTicker(state.Ticker.Value);
                    }
                    else if (state.State == PlayerTurnState.LocalState.Outside)
                    {
                        drawMap();
                        drawTicker(state.Ticker.Value);
                    }
                }
            }
           
            Surface scaledBuffer =
                buffer.CreateScaledSurface(bufferScale, antiAlias);
            
            screen.Blit(
                scaledBuffer,
                new Point(
                    screen.Width / 2 - scaledBuffer.Width / 2,
                    screen.Height / 2 - scaledBuffer.Height / 2
                    ));

            destroySurface(scaledBuffer);

            screen.Update();

            isDrawing = false;
        }

        private void drawMapHeaderText(string text)
        {
            TextSprite txt = getTextSprite(text, 18, Color.Black);

            buffer.Blit(
                txt,
                new Point(
                    buffer.Width / 2 - txt.Size.Width / 2,
                    VERTICALBUFFER / 2 - txt.Size.Height / 2
                    ));
        }

        private void drawMapFooterText(string text)
        {
            TextSprite txt = getTextSprite(text, 18, Color.Black);

            buffer.Blit(
                txt,
                new Point(
                    buffer.Width / 2 - txt.Size.Width / 2,
                    buffer.Height - VERTICALBUFFER / 2 - txt.Size.Height - 5
                    ));
        }

        private void drawMapStatusText(string text)
        {
            TextSprite txt = getTextSprite(text, 20, Color.Black);

            buffer.Blit(
                txt,
                new Point(
                    buffer.Width / 2 - txt.Size.Width / 2,
                    buffer.Height - VERTICALBUFFER / 2 + 5
                    ));
        }

        private void drawStore(int playerNum, PlayerSprite sprite, MuleSprite muleSprite, bool hasMule)
        {
            storeBuffer.Blit(storebg);

            for (int i = 0; i < 4; i++)
            {
                storeBuffer.Blit(
                    departments,
                    new Point(
                        STOREBUFFER + i * (DEPTWIDTH + DEPTSPACING),
                        STOREBUFFER
                        ),
                    new Rectangle(
                        i * DEPTWIDTH * 2 + DEPTWIDTH,
                        0,
                        DEPTWIDTH,
                        DEPTTOPHEIGHT
                        ));

                if (i < 3)
                    storeBuffer.Blit(
                        departments,
                        new Point(
                            STOREBUFFER + i * (DEPTWIDTH + DEPTTIGHTSPACING),
                            STOREHEIGHT - STOREBUFFER - DEPTBOTTOMHEIGHT
                            ),
                        new Rectangle(
                            i * DEPTWIDTH * 2 + DEPTWIDTH,
                            DEPTTOPHEIGHT + DEPTBUFFER,
                            DEPTWIDTH,
                            DEPTBOTTOMHEIGHT
                            ));
                else
                    storeBuffer.Blit(
                        departments,
                        new Point(
                            STOREWIDTH - STOREBUFFER - DEPTMULEWIDTH,
                            STOREHEIGHT - STOREBUFFER - DEPTMULEHEIGHT
                            ),
                        new Rectangle(
                            i * DEPTWIDTH * 2,
                            DEPTTOPHEIGHT + DEPTBUFFER,
                            DEPTMULEWIDTH,
                            DEPTMULEHEIGHT
                            ));
            }

            for (int i = 0; i < game.Store.Mules; i++)
            {
                if (i == 14) break;

                storeBuffer.Blit(
                    departments,
                    new Point(
                        STOREWIDTH -
                        STOREBUFFER -
                        MULEICONHBUFFER -
                        (2 - (i % 2)) * MULEICONWIDTH,
                        STOREHEIGHT -
                        STOREBUFFER -
                        MULEICONVBUFFER -
                        ((i + 2) / 2) * MULEICONHEIGHT
                        ),
                    new Rectangle(
                        6 * DEPTWIDTH + DEPTMULEWIDTH,
                        DEPTTOPHEIGHT + DEPTBUFFER,
                        MULEICONWIDTH,
                        MULEICONHEIGHT
                        ));

            }

            TextSprite txt =
                getTextSprite(
                    "$" + game.Store.MulePrice.ToString(),
                    18,
                    Color.Black
                    );

            storeBuffer.Blit(
                txt,
                new Point(
                    STOREWIDTH -
                    STOREBUFFER -
                    DEPTMULEWIDTH / 2 -
                    txt.Width / 2,
                    STOREHEIGHT -
                    STOREBUFFER -
                    MULEICONVBUFFER / 2 -
                    txt.Height / 2
                    ));

            txt =
                getTextSprite(
                    "MULES",
                    18,
                    Color.White
                    );

            storeBuffer.Blit(
                txt,
                new Point(
                    STOREWIDTH -
                    STOREBUFFER -
                    DEPTMULEWIDTH / 2 -
                    txt.Width / 2,
                    STOREHEIGHT -
                    STOREBUFFER / 2 -
                    txt.Height / 2
                    ));

            if (hasMule)
                drawMule(
                    storeBuffer,
                    playerNum,
                    muleSprite.Outfitted,
                    muleSprite.Frame,
                    muleSprite.X,
                    muleSprite.Y,
                    false
                    );

            drawPlayer(
                storeBuffer,
                playerNum,
                sprite.Frame,
                sprite.X,
                sprite.Y,
                false
                );

            for (int i = 0; i < 4; i++)
            {
                storeBuffer.Blit(
                    departments,
                    new Point(
                        STOREBUFFER + i * (DEPTWIDTH + DEPTSPACING),
                        STOREBUFFER
                        ),
                    new Rectangle(
                        i * DEPTWIDTH * 2,
                        0,
                        DEPTWIDTH,
                        DEPTTOPHEIGHT
                        ));

                if (i < 3)
                    storeBuffer.Blit(
                        departments,
                        new Point(
                            STOREBUFFER + i * (DEPTWIDTH + DEPTTIGHTSPACING),
                            STOREHEIGHT - STOREBUFFER - DEPTBOTTOMHEIGHT
                            ),
                        new Rectangle(
                            i * DEPTWIDTH * 2,
                            DEPTTOPHEIGHT + DEPTBUFFER,
                            DEPTWIDTH,
                            DEPTBOTTOMHEIGHT
                            ));
            }

            buffer.Blit(
                storeBuffer,
                new Point(
                    buffer.Width / 2 - storeBuffer.Width / 2,
                    buffer.Height / 2 - storeBuffer.Height / 2
                    ));
        }

        private void drawSummary()
        {
            SummaryState state = (SummaryState)game.State;

            if (state.State == SummaryState.LocalState.DisplayWait)
            {
                TextSprite txt =
                    getTextSprite(
                        "STATUS SUMMARY #" + game.Round.ToString(),
                        22,
                        Color.Yellow
                        );

                buffer.Blit(
                    txt,
                    new Point(
                        game.Width / 2 - txt.Width / 2,
                        SummaryState.SCREENBUFFER + 
                        SummaryState.VERTICALBUFFER / 2 -
                        txt.Height / 2
                        ));

                txt =
                    getTextSprite(
                        "COLONY    " + game.GetColonyTotal().ToString(),
                        22,
                        Color.Yellow
                        );

                buffer.Blit(
                    txt,
                    new Point(
                        game.Width / 2 - txt.Width / 2,
                        game.Height -
                        SummaryState.SCREENBUFFER -
                        SummaryState.VERTICALBUFFER / 2 -
                        txt.Height
                        ));

                txt =
                    getTextSprite(
                        "PRESS ALL PLAYER BUTTONS TO GO ON.",
                        18,
                        Color.White
                        );

                buffer.Blit(
                    txt,
                    new Point(
                        game.Width / 2 - txt.Width / 2,
                        game.Height -
                        SummaryState.SCREENBUFFER -
                        SummaryState.VERTICALBUFFER / 2 +
                        10
                        ));
            }

            Player[] players = game.OrderedPlayers;

            for (int i = 0; i < players.Length; i++)
            {
                drawPlayer(
                    buffer,
                    players[i].PlayerNum,
                    state.Sprites[i].Frame,
                    state.Sprites[i].X,
                    state.Sprites[i].Y,
                    state.Sprites[i].Small
                    );

                if (state.State == SummaryState.LocalState.DisplayWait)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        string text = string.Empty;

                        switch (j)
                        {
                            case 0: text = players[i].Name; break;
                            case 1: text = "MONEY"; break;
                            case 2: text = "LAND"; break;
                            case 3: text = "GOODS"; break;
                            case 4: text = "TOTAL"; break;
                        }

                        TextSprite txt =
                            getTextSprite(
                                text,
                                18,
                                (
                                    state.PlayerReady[players[i].PlayerNum] && !players[i].AI ?
                                    Color.Yellow :
                                    getPlayerColor(players[i].Color)
                                ));

                        buffer.Blit(
                            txt,
                            new Point(
                                state.Sprites[i].X + PLAYERWIDTH + 10,
                                state.Sprites[i].Y + j * (txt.Height + 10)
                                ));

                        if (j > 0)
                        {
                            switch (j)
                            {
                                case 1: text = players[i].Money.ToString(); break;
                                case 2: text = game.GetLandValue(players[i].PlayerNum).ToString(); break;
                                case 3: text = game.GetGoodsValue(players[i].PlayerNum).ToString(); break;
                                case 4: text = game.GetTotalValue(players[i].PlayerNum).ToString(); break;
                            }

                            txt =
                                getTextSprite(
                                    text,
                                    18,
                                    (
                                        state.PlayerReady[players[i].PlayerNum] && !players[i].AI ?
                                        Color.Yellow :
                                        getPlayerColor(players[i].Color)
                                    ));

                            buffer.Blit(
                                txt,
                                new Point(
                                    state.Sprites[i].X +
                                    (buffer.Width - SummaryState.SCREENBUFFER * 2) / 2 -
                                    100 - txt.Width,
                                    state.Sprites[i].Y + j * (txt.Height + 10)
                                    ));
                        }
                    }
                }
            }
        }

        private void drawTicker(double value)
        {
            buffer.Draw(
                new Box(
                    new Point(
                        buffer.Width - HORIZONTALBUFFER / 2 - TIMERWIDTH / 2,
                        buffer.Height / 2 + TIMERHEIGHT / 2 - 
                        (int)(TIMERHEIGHT * (value / Ticker.MAXVALUE))
                        ),
                    new Size(
                        TIMERWIDTH,
                        (int)(TIMERHEIGHT * (value / Ticker.MAXVALUE))
                        )),
                    Color.Gray,
                    false,
                    true
                    );
        }

        private void drawMap()
        {
            mapBuffer.Blit(map);

            for (int y = 0; y < numTilesHigh; y++)
                for (int x = 0; x < numTilesWide; x++)
                {
                    if (game.Map.Plots[x, y].Player > -1)
                    {
                        mapBuffer.Blit(
                            plots,
                            new Point(
                                x * PLOTSIZE,
                                y * PLOTSIZE
                                ),
                            new Rectangle(
                                (int)game.Map.Plots[x, y].Resource * PLOTSIZE, 
                                (
                                    game.Players[game.Map.Plots[x, y].Player].Highlight ?
                                    0 :
                                    (int)game.Players[game.Map.Plots[x, y].Player].Color * PLOTSIZE
                                ), 
                                PLOTSIZE, 
                                PLOTSIZE
                                ));
                    }
                }

            if (game.State is TransportState)
            {
                TransportState state = (TransportState)game.State;

                drawShip(
                    ShipType.Mothership,
                    state.ShipRectangle.X,
                    state.ShipRectangle.Y,
                    state.ShipRectangle.Width,
                    state.ShipRectangle.Height
                    );
            }
            else if (game.State is LandGrantState)
            {
                LandGrantState state = (LandGrantState)game.State;

                if (state.State == LandGrantState.LocalState.Normal ||
                    state.State == LandGrantState.LocalState.GainedPlot)
                    mapBuffer.Blit(
                        cursor,
                        new Point(
                            state.CursorPos.X * PLOTSIZE,
                            state.CursorPos.Y * PLOTSIZE
                            ),
                        new Rectangle(
                            0,
                            (
                                state.State == LandGrantState.LocalState.Normal ?
                                0 :
                                (int)game.Players[
                                        game.Map.Plots[state.CursorPos.X, state.CursorPos.Y].Player
                                    ].Color * PLOTSIZE
                            ),
                            PLOTSIZE,
                            PLOTSIZE
                            ));

            }
            else if (game.State is PlayerTurnState)
            {
                PlayerTurnState state = (PlayerTurnState)game.State;

                if (state.HasMule)
                    drawMule(
                        mapBuffer,
                        state.PlayerNum,
                        state.MuleSprite.Outfitted,
                        state.MuleSprite.Frame,
                        state.MuleSprite.X,
                        state.MuleSprite.Y,
                        state.MuleSprite.Small
                        );

                drawPlayer(
                    mapBuffer,
                    state.PlayerNum,
                    state.PlayerSprite.Frame,
                    state.PlayerSprite.X,
                    state.PlayerSprite.Y,
                    state.PlayerSprite.Small
                    );
            }

            Surface scaledMap =
                mapBuffer.CreateScaledSurface(mapScale, antiAlias);

            buffer.Blit(
                scaledMap,
                new Point(
                    buffer.Width / 2 - scaledMap.Width / 2,
                    buffer.Height / 2 - scaledMap.Height / 2
                    ));

            destroySurface(scaledMap);
        }

        private void drawShip(ShipType type, int x, int y, int width, int height)
        {
            if (width < SHIPWIDTH ||
                height < SHIPHEIGHT)
            {
                Surface scaledShip =
                    ships.CreateScaledSurface((double)width / (double)SHIPWIDTH, antiAlias);
                scaledShip.Transparent = true;
                scaledShip.TransparentColor = Color.FromArgb(255, 0, 255);

                mapBuffer.Blit(
                    scaledShip,
                    new Point(x, y),
                    new Rectangle(
                        0, 0,
                        width,
                        height
                        ));

                destroySurface(scaledShip);
            }
            else
                mapBuffer.Blit(
                    ships,
                    new Point(x, y),
                    new Rectangle(
                        0, 0, SHIPWIDTH, SHIPHEIGHT
                        ));
        }

        private void drawPlayer(Surface dest, int playerNum, int frame, int x, int y, bool small)
        {
            if (small)            
                dest.Blit(
                    charsSmall[(int)(game.Players[playerNum].Type) - 1],
                    new Point(x, y),
                    new Rectangle(
                        frame * (SMALLPLAYERWIDTH + SMALLPLAYERBUFFER),
                        (
                            game.Players[playerNum].Highlight ?
                            0 :
                            (int)(game.Players[playerNum].Color) * SMALLPLAYERHEIGHT
                        ),
                        SMALLPLAYERWIDTH,
                        SMALLPLAYERHEIGHT
                        ));
            else
                dest.Blit(
                    chars[(int)(game.Players[playerNum].Type) - 1],
                    new Point(x, y),
                    new Rectangle(
                        frame * (PLAYERWIDTH + PLAYERBUFFER),
                        (
                            game.Players[playerNum].Highlight ?
                            0 :
                            (int)(game.Players[playerNum].Color) * PLAYERHEIGHT
                        ),
                        PLAYERWIDTH,
                        PLAYERHEIGHT
                        ));
        }

        private void drawMule(
            Surface dest, 
            int playerNum,
            bool outfitted, 
            int frame, 
            int x, int y, 
            bool small)
        {
            if (small)
                dest.Blit(
                    muleSmall,
                    new Point(x, y),
                    new Rectangle(
                        frame * (SMALLMULEWIDTH + SMALLMULEBUFFER),
                        (
                            !outfitted ?
                            0 :
                            (int)(game.Players[playerNum].Color) * SMALLMULEHEIGHT
                        ),
                        SMALLMULEWIDTH,
                        SMALLMULEHEIGHT
                        ));
            else
                dest.Blit(
                    mule,
                    new Point(x, y),
                    new Rectangle(
                        frame * (MULEWIDTH + MULEBUFFER),
                        (
                            !outfitted ?
                            0 :
                            (int)(game.Players[playerNum].Color) * MULEHEIGHT
                        ),
                        MULEWIDTH,
                        MULEHEIGHT
                        ));
        }
    }
}