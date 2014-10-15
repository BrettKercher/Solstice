using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Solstice
{


    class Camera
	    {
	        protected float cameraZoomValue; // Camera Zoom
	        public Matrix cameraTransformValue; // Matrix Transform
	        public Vector2 cameraPosition; // Camera Position
	        protected float cameraRotationValue; // Camera Rotation
            Viewport view;
            Vector2 center;


            public Vector2 Center
            {
                get { return center; }
            }
            
            public Camera(GraphicsDevice graphicsDevice)
	        {
                view = graphicsDevice.Viewport;
                cameraZoomValue = 1.0f;
	            cameraRotationValue = 0.0f;
	            cameraPosition = Vector2.Zero;
	        }
	 
	        public float Zoom
	        {
	            get { return cameraZoomValue; }
	            set { cameraZoomValue = value; if (cameraZoomValue < 0.1f) cameraZoomValue = 0.1f; } // Negative zoom will flip image
	        }
	 
	        public float Rotation
	        {
	            get { return cameraRotationValue; }
	            set { cameraRotationValue = value; }
	        }


            public void Update(float playerPositionX, float playerPositionY, int playerWidth, int playerHeight, Vector2 dimensions, Map map)
            {

                if ((playerPositionX + (playerWidth / 2)) >= (((map.Dimensions.X) * map.TileSize) - (view.Width / 2)))
                {
                    center.X = ((map.Dimensions.X * map.TileSize) - view.Width);
                }
                else if ((playerPositionX + (playerWidth / 2)) < (view.Width / 2))
                {
                    center.X = 0;
                }
                else
                    center.X = playerPositionX + (playerWidth / 2) - (dimensions.X / 2);


                if ((playerPositionY + (playerHeight / 2)) >= (((map.Dimensions.Y) * map.TileSize) - (view.Height / 2)))
                {
                    center.Y = ((map.Dimensions.Y * map.TileSize) - view.Height);
                }
                else if ((playerPositionY + (playerHeight / 2)) < (view.Height / 2))
                {
                    center.Y = 0;
                }
                else
                    center.Y = playerPositionY + (playerHeight / 2) - (dimensions.Y / 2);

                cameraPosition = new Vector2(center.X + view.Width / 2, center.Y + view.Height / 2);
            }

	        // Auxiliary function to move the camera
	        public void Move(Vector2 amount)
	        {
	            cameraPosition += amount;
	        }
	        // Get set position
	        public Vector2 Position
	        {
	            get { return cameraPosition; }
	            set { cameraPosition = value; }
	        }
	 
	        public Matrix get_transformation(GraphicsDevice graphicsDevice)
	        {
	            cameraTransformValue =
	              Matrix.CreateTranslation(new Vector3(-cameraPosition.X, -cameraPosition.Y, 0)) *
	                                         Matrix.CreateRotationZ(Rotation) *
	                                         Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) *
	                                         Matrix.CreateTranslation(new Vector3(graphicsDevice.Viewport.Width * 0.5f, graphicsDevice.Viewport.Height * 0.5f, 0));
	            return cameraTransformValue;
	        }
	    }

















    //class Camera
    //{
    //    public Matrix transform;
    //    Viewport view;
    //    Vector2 center;

    //    public Matrix Transform
    //    {
    //        get { return transform; }
    //    }
    //    public Vector2 Center
    //    {
    //        get { return cameraPosition; }
    //    }

    //    public Camera(Viewport newView)
    //    {
    //        view = newView;
    //    }

    //    public void Update(float playerPositionX, float playerPositionY, int playerWidth, int playerHeight, Vector2 dimensions, Map map)
    //    {

    //        if ((playerPositionX + (playerWidth / 2)) >= (((map.Dimensions.X) * map.TileSize) - (view.Width / 2)))
    //        {
    //            center.X = ((map.Dimensions.X * map.TileSize) - view.Width);
    //        }
    //        else if ((playerPositionX + (playerWidth / 2)) < (view.Width / 2))
    //        {
    //            center.X = 0;
    //        }
    //        else
    //            center.X = playerPositionX + (playerWidth / 2) - (dimensions.X / 2);


    //        if ((playerPositionY + (playerHeight / 2)) >= (((map.Dimensions.Y) * map.TileSize) - (view.Height / 2)))
    //        {
    //            center.Y = ((map.Dimensions.Y * map.TileSize) - view.Height);
    //        }
    //        else if ((playerPositionY + (playerHeight / 2)) < (view.Height / 2))
    //        {
    //            center.Y = 0;
    //        }
    //        else
    //            center.Y = playerPositionY + (playerHeight / 2) - (dimensions.Y / 2);

    //        transform = Matrix.CreateScale(new Vector3(1, 1, 0)) * Matrix.CreateTranslation(new Vector3(-center.X, -center.Y, 0));
    //    }


    //}
}
