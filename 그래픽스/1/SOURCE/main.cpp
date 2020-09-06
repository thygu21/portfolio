#include "glSetup.h"

#include <glm/glm.hpp>				// OpenGL Mathematics
#include <glm/gtc/type_ptr.hpp>		// glm::value_ptr()
using namespace glm;

#include <iostream>
#include <algorithm>
#define _USE_MATH_DEFINES			//using M_PI
#include <math.h>			
using namespace std;

void init();
void render(GLFWwindow* window);
void keyboard(GLFWwindow* window, int key, int code, int action, int mods);
void setDot();
void drawDot();
void setFrame(int f);
void moveAxisY(int n);
void moveAxisShaft(int n);
void setNormalVector();
void drawNormalVector(bool toggle);

// Camera configuation
vec3 eye(3.0, 3, 3.5);
vec3 center(0, 0, 0);
vec3 up(0, 1, 0);

// Global coordinate frame
float AXIS_LENGTH = 3;
float AXIS_LINE_WIDTH = 2;

// Colors
GLfloat bgColor[4] = { 1,1,1,1 };

// Selected example
int selection = 1;

// position p
vec4 p[36][18];

//normal vector & mid position vector
vec3 nor[36][18]; 
vec3 midPoint[36][18];

// rotate matrix
glm::mat4 axisZ = glm::mat4(1.0);
glm::mat4 axisY = glm::mat4(1.0);

// translate matrix
glm::mat4 transOrigin = glm::mat4(1.0);
glm::mat4 transPivot = glm::mat4(1.0);

// sweep value
int ySweep = 36;
int zSweep = 18;

// normalVector toggle
bool toggle = false;

int main(int argc, char* argv[])
{
	// Initialize the OpenGL system
	GLFWwindow* window = initializeOpenGL(argc, argv, bgColor);
	if (window == NULL) return -1;

	// Callbacks
	glfwSetKeyCallback(window, keyboard);

	// Depth test
	glEnable(GL_DEPTH_TEST);

	// Normal vectors are normalized after transformation.
	glEnable(GL_NORMALIZE);

	// Viewport and perspective setting
	reshape(window, windowW, windowH);

	// Initialization - Main loop - Finalization
	//
	init();

	// ---Main loop---

	while (!glfwWindowShouldClose(window))
	{
		glfwPollEvents();			// Events
		render(window);				// Draw one frame
		glfwSwapBuffers(window);	// Swap buffers
	}

	// Terminate the glfw system
	glfwDestroyWindow(window);
	glfwTerminate();

	return 0;
}

void setDot()
{
	glColor3f(0, 0, 0);
	p[0][0] = vec4(1.2, 1, 0, 1.0f); //start dot
	transOrigin = glm::translate(transOrigin, vec3(-1.0, -1.0, 0));
	transPivot = glm::translate(transPivot, vec3(1.0, 1.0, 0));
	for (int j = 0; j < 17; j++)
	{
		axisZ = glm::rotate(axisZ, glm::radians(20.0f), vec3(0, 0, 1.0));
		p[0][j + 1] = transPivot * axisZ * transOrigin * p[0][0];
	}
	for (int i = 0; i < 35; i++)
	{
		axisY = glm::rotate(axisY, glm::radians(10.0f), vec3(0, 1, 0));
		for (int j = 0; j < 18; j++)
		{
			p[i + 1][j] = axisY * p[0][j];
		}
	}
}

void drawDot()
{
	glColor3f(0, 0, 0);
	glBegin(GL_POINTS);
	{
		for (int i = 0; i < ySweep; i++)
		{
			for (int j = 0; j < zSweep; j++)
			{
				glVertex3f(p[i][j].x, p[i][j].y, p[i][j].z);
			}
		}
	}
	glEnd();
}

void setFrame(int f)
{
	int ys, zs; //temp ySweep, zSweep for last sweep
	vec3 v; // midPoint to eye;

	if (f == 1) // frame
	{
		glPolygonOffset(0.0, 0.0);
		glEnable(GL_POLYGON_OFFSET_FILL);
		glPolygonMode(GL_FRONT_AND_BACK, GL_LINE);
		glColor3f(0, 0, 0); // black
	}
	else // fill
	{
		glPolygonOffset(1.0, 1.0);
		glEnable(GL_POLYGON_OFFSET_FILL);
		glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);
		glColor3f(0, 0, 1); // blue
	}

	// distinguish full sweep or not
	if (ySweep == 36)
		ys = ySweep - 1;
	else
		ys = ySweep;
	if (zSweep == 18)
		zs = zSweep - 1;
	else
		zs = zSweep;

	for (int i = 0; i < ys; i++)
	{
		for (int j = 0; j < zs; j++)
		{
			glBegin(GL_QUADS); //basic draw
			{
				if (f == 3) // two side
				{
					v = eye - midPoint[i][j];
					if (dot(v, nor[i][j]) < 0)
						glColor3f(1, 0, 0);
					else
						glColor3f(0, 0, 1);
				}

				glVertex3f(p[i][j].x, p[i][j].y, p[i][j].z);
				glVertex3f(p[i][j+1].x, p[i][j+1].y, p[i][j+1].z);
				glVertex3f(p[i + 1][j + 1].x, p[i + 1][j + 1].y, p[i + 1][j + 1].z);
				glVertex3f(p[i+1][j].x, p[i+1][j].y, p[i+1][j].z);
			}
			glEnd();
			if (ySweep == 36)
			{
				glBegin(GL_QUADS); //i 0 to 35 connect in j 0 to 16
				{
					if (f == 3) // two side
					{
						v = eye - midPoint[35][j];
						if (dot(v, nor[35][j]) < 0)
							glColor3f(1, 0, 0);
						else
							glColor3f(0, 0, 1);
					}

					glVertex3f(p[35][j].x, p[35][j].y, p[35][j].z);
					glVertex3f(p[35][j + 1].x, p[35][j + 1].y, p[35][j + 1].z);
					glVertex3f(p[0][j + 1].x, p[0][j + 1].y, p[0][j + 1].z);
					glVertex3f(p[0][j].x, p[0][j].y, p[0][j].z);
				}
				glEnd();
			}
		}
		if (zSweep == 18)
		{
			glBegin(GL_QUADS); //j 0 to 17 connect in i 0 to 34
			{
				if (f == 3) // two side
				{
					v = eye - midPoint[i][17];
					if (dot(v, nor[i][17]) < 0)
						glColor3f(1, 0, 0);
					else
						glColor3f(0, 0, 1);
				}

				glVertex3f(p[i][0].x, p[i][0].y, p[i][0].z);
				glVertex3f(p[i][17].x, p[i][17].y, p[i][17].z);
				glVertex3f(p[i + 1][17].x, p[i + 1][17].y, p[i + 1][17].z);
				glVertex3f(p[i + 1][0].x, p[i + 1][0].y, p[i + 1][0].z);
			}
			glEnd();
		}
	}

	if (ySweep == 36 && zSweep == 18)
	{
		glBegin(GL_QUADS); //j 0 to 17 and i 0 to 35 draw
		{
			if (f == 3) // two side
			{
				v = eye - midPoint[35][17];
				if (dot(v, nor[35][17]) < 0)
					glColor3f(1, 0, 0);
				else
					glColor3f(0, 0, 1);
			}

			glVertex3f(p[0][0].x, p[0][0].y, p[0][0].z);
			glVertex3f(p[0][17].x, p[0][17].y, p[0][17].z);
			glVertex3f(p[35][17].x, p[35][17].y, p[35][17].z);
			glVertex3f(p[35][0].x, p[35][0].y, p[35][0].z);
		}
		glEnd();
	}
}

//a, s move
void moveAxisY(int n)
{
	if (n == 1) // a
	{
		if (ySweep < 36)
			ySweep++;
	}
	else // s
	{
		if (ySweep > 0)
			ySweep--;
	}
}

//j, k move
void moveAxisShaft(int n)
{
	if (n == 1) // j
	{
		if (zSweep < 18)
			zSweep++;
	}
	else // k
	{
		if (zSweep > 0)
			zSweep--;
	}
}

void setNormalVector()
{
	int ys, zs; //temp ySweep, zSweep for last sweep
	vec3 v1, v2, c, add, mid;
	double normalize;

	// distinguish full sweep or not
	if (ySweep == 36)
		ys = ySweep - 1;
	else
		ys = ySweep;
	if (zSweep == 18)
		zs = zSweep - 1;
	else
		zs = zSweep;

	for (int i = 0; i < ys; i++)
	{
		for (int j = 0; j < zs; j++)
		{
			//cross
			v1 = p[i + 1][j] - p[i][j];
			v2 = p[i][j + 1] - p[i][j];
			c = glm::cross(v1, v2);

			//normalize
			normalize = sqrt(c.x * c.x + c.y * c.y + c.z * c.z) * 8.0; // for visible
			c = vec3(c.x / normalize, c.y / normalize, c.z / normalize);
			nor[i][j] = c;

			add = p[i][j] + p[i + 1][j + 1];
			mid = vec3(add.x / 2.0, add.y / 2.0, add.z / 2.0);
			midPoint[i][j] = mid;
			if (ySweep == 36) //i 0 to 35 connect in j 0 to 16
			{
				//cross
				v1 = p[0][j] - p[35][j];
				v2 = p[35][j + 1] - p[35][j];
				c = glm::cross(v1, v2);

				//normalize
				normalize = sqrt(c.x * c.x + c.y * c.y + c.z * c.z) * 8.0; // for visible
				c = vec3(c.x / normalize, c.y / normalize, c.z / normalize);
				nor[35][j] = c;

				add = p[35][j] + p[0][j + 1];
				mid = vec3(add.x / 2.0, add.y / 2.0, add.z / 2.0);
				midPoint[35][j] = mid;
			}
		}
		if (zSweep == 18) //j 0 to 17 connect in i 0 to 34
		{
			//cross
			v1 = p[i + 1][17] - p[i][17];
			v2 = p[i][0] - p[i][17];
			c = glm::cross(v1, v2);

			//normalize
			normalize = sqrt(c.x * c.x + c.y * c.y + c.z * c.z) * 8.0; // for visible
			c = vec3(c.x / normalize, c.y / normalize, c.z / normalize);
			nor[i][17] = c;

			add = p[i][0] + p[i + 1][17];
			mid = vec3(add.x / 2.0, add.y / 2.0, add.z / 2.0);
			midPoint[i][17] = mid;
		}
	}

	if (ySweep == 36 && zSweep == 18) //j 0 to 17 and i 0 to 35 draw
	{
		//cross
		v1 = p[0][17] - p[35][17];
		v2 = p[35][0] - p[35][17];
		c = glm::cross(v1, v2);

		//normalize
		normalize = sqrt(c.x * c.x + c.y * c.y + c.z * c.z) * 8.0; // for visible
		c = vec3(c.x / normalize, c.y / normalize, c.z / normalize);
		nor[35][17] = c;

		add = p[35][17] + p[0][0];
		mid = vec3(add.x / 2.0, add.y / 2.0, add.z / 2.0);
		midPoint[35][17] = mid;
	}
}

void drawNormalVector(bool toggle)
{
	if (toggle)
	{
		glColor3f(0, 0, 0);

		for (int i = 0; i < ySweep; i++)
		{
			for (int j = 0; j < zSweep; j++)
			{
				glBegin(GL_LINES); //basic draw
				{
					glVertex3f(midPoint[i][j].x, midPoint[i][j].y, midPoint[i][j].z);
					glVertex3f(midPoint[i][j].x + nor[i][j].x, midPoint[i][j].y + nor[i][j].y, midPoint[i][j].z + nor[i][j].z);
				}
				glEnd();
			}
		}
	}
}

void init()
{
	// dot set
	setDot();

	// vector set
	setNormalVector();
}

void render(GLFWwindow* window)
{
	// Background color
	glClearColor(bgColor[0], bgColor[1], bgColor[2], bgColor[3]);
	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

	// Modelview matrix
	glMatrixMode(GL_MODELVIEW);
	glLoadIdentity();

	glLineWidth(3 * dpiScaling);
	glPointSize(2.0f);

	gluLookAt(eye[0], eye[1], eye[2], center[0], center[1], center[2], up[0], up[1], up[2]);

	// Axes
	glDisable(GL_LIGHTING);
	drawAxes(AXIS_LENGTH, AXIS_LINE_WIDTH*dpiScaling);
	
	// Draw
	switch (selection)
	{
	case 1: drawDot();	drawNormalVector(toggle);				break;
	case 2: setFrame(1); drawNormalVector(toggle);				break;
	case 3: setFrame(2); drawNormalVector(toggle);				break;
	case 4: setFrame(1); setFrame(2); drawNormalVector(toggle);	break;
	case 6: setFrame(1); setFrame(3); drawNormalVector(toggle); break;
	}
}

void keyboard(GLFWwindow* window, int key, int scancode, int action, int mods)
{
	if (action == GLFW_PRESS || action == GLFW_REPEAT)
	{
		switch (key)
		{
			// Quit
		case GLFW_KEY_Q:
		case GLFW_KEY_ESCAPE: glfwSetWindowShouldClose(window, GL_TRUE); break;
			
			// Example selection
		case GLFW_KEY_1: selection = 1; break;
		case GLFW_KEY_2: selection = 2; break;
		case GLFW_KEY_3: selection = 3; break;
		case GLFW_KEY_4: selection = 4; break;
		case GLFW_KEY_5: 
		{
			if (toggle)
				toggle = false;
			else
				toggle = true;
			break;
		}
		case GLFW_KEY_6: selection = 6; break;

			// movement
		case GLFW_KEY_A: moveAxisY(1); break;
		case GLFW_KEY_S: moveAxisY(2); break;
		case GLFW_KEY_J: moveAxisShaft(1); break;
		case GLFW_KEY_K: moveAxisShaft(2); break;
		}
	}
}