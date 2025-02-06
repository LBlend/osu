// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    public partial class StarsGroupPanel : PoolableDrawable, ICarouselPanel
    {
        public const float HEIGHT = CarouselItem.DEFAULT_HEIGHT;

        private const float duration = 500;

        [Resolved]
        private BeatmapCarousel? carousel { get; set; }

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        private CarouselPanelPiece panel = null!;
        private Drawable chevronIcon = null!;
        private Box contentBackground = null!;
        private StarRatingDisplay starRatingDisplay = null!;
        private StarCounter starCounter = null!;

        protected override bool ReceivePositionalInputAtSubTree(Vector2 screenSpacePos)
        {
            var inputRectangle = panel.TopLevelContent.DrawRectangle;

            // Cover a gap introduced by the spacing between a GroupPanel and other panel types either below/above it.
            inputRectangle = inputRectangle.Inflate(new MarginPadding { Vertical = BeatmapCarousel.SPACING / 2f });

            return inputRectangle.Contains(panel.TopLevelContent.ToLocalSpace(screenSpacePos));
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Anchor = Anchor.TopRight;
            Origin = Anchor.TopRight;
            RelativeSizeAxes = Axes.X;
            Height = HEIGHT;

            InternalChild = panel = new CarouselPanelPiece(0)
            {
                Icon = chevronIcon = new SpriteIcon
                {
                    AlwaysPresent = true,
                    Icon = FontAwesome.Solid.ChevronDown,
                    Size = new Vector2(12),
                    Margin = new MarginPadding { Horizontal = 5f },
                    X = 2f,
                },
                Background = contentBackground = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Dark1,
                },
                AccentColour = colourProvider.Highlight1,
                Children = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        AutoSizeAxes = Axes.Both,
                        Spacing = new Vector2(10f, 0f),
                        Margin = new MarginPadding { Left = 10f },
                        Children = new Drawable[]
                        {
                            starRatingDisplay = new StarRatingDisplay(default, StarRatingDisplaySize.Small)
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                            },
                            starCounter = new StarCounter
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Scale = new Vector2(8f / 20f),
                            },
                        }
                    },
                    new CircularContainer
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        Size = new Vector2(50f, 14f),
                        Margin = new MarginPadding { Right = 20f },
                        Masking = true,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.Black.Opacity(0.7f),
                            },
                            new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Font = OsuFont.Torus.With(size: 14.4f, weight: FontWeight.Bold),
                                // TODO: requires Carousel/CarouselItem-side implementation
                                Text = "43",
                                UseFullGlyphHeight = false,
                            }
                        },
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Expanded.BindValueChanged(_ => onExpanded(), true);
            KeyboardSelected.BindValueChanged(k => panel.KeyboardActive.Value = k.NewValue, true);
        }

        private void onExpanded()
        {
            panel.Active.Value = Expanded.Value;
            panel.Flash();

            chevronIcon.ResizeWidthTo(Expanded.Value ? 12f : 0f, duration, Easing.OutQuint);
            chevronIcon.FadeTo(Expanded.Value ? 1f : 0f, duration, Easing.OutQuint);
        }

        protected override void PrepareForUse()
        {
            base.PrepareForUse();

            Debug.Assert(Item != null);

            StarsGroupDefinition group = (StarsGroupDefinition)Item.Model;

            Color4 colour = group.StarNumber >= 9 ? OsuColour.Gray(0.2f) : colours.ForStarDifficulty(group.StarNumber);
            Color4 contentColour = group.StarNumber >= 7 ? colours.Orange1 : colourProvider.Background5;

            panel.AccentColour = colour;
            contentBackground.Colour = colour.Darken(0.3f);

            starRatingDisplay.Current.Value = new StarDifficulty(group.StarNumber, 0);
            starCounter.Current = group.StarNumber;

            chevronIcon.Colour = contentColour;
            starCounter.Colour = contentColour;

            this.FadeInFromZero(500, Easing.OutQuint);
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (carousel != null)
                carousel.CurrentSelection = Item!.Model;

            return true;
        }

        #region ICarouselPanel

        public CarouselItem? Item { get; set; }
        public BindableBool Selected { get; } = new BindableBool();
        public BindableBool Expanded { get; } = new BindableBool();
        public BindableBool KeyboardSelected { get; } = new BindableBool();

        public double DrawYPosition { get; set; }

        public void Activated()
        {
            // sets should never be activated.
            throw new InvalidOperationException();
        }

        #endregion
    }
}
