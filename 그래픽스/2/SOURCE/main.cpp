#include "glSetup.h"

#include <glm/glm.hpp>				// OpenGL Mathematics
#include <glm/gtc/type_ptr.hpp>		// glm::value_ptr()
using namespace glm;

#include <iostream>
#include <algorithm>
#define _USE_MATH_DEFINES			//using M_PI
#include <math.h>			
using namespace std;

// Camera configuation
vec3 eye(5.5, 5, 5.5);
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

// Light configuration
vec4 lightInitialP(0.0, 2.5, 2, 1);	// Initial light position

float thetaModel = 0;			// Rotation angle around the y-axis
float thetaLight[3];

bool lightOn[3];				// Point = 0, distant = 1, spot = 2 lights
bool attenuation = false;		// Attenuation for point light

bool exponent = false;
float exponentInitial = 30.0;				// [0, 128]
float exponentValue = exponentInitial;
float exponentNorm = exponentValue / 128.0; // [0, 1]

bool cutoff = false;
float cutoffMax = 60;						// [0, 90] degree
float cutoffInitial = 20.0;					// [0, cutoffMax] degree
float cutoffValue = cutoffInitial;
float cutoffNorm = cutoffValue / cutoffMax; // [0, 1]

// Play configuration
bool pause = false;

float timeStep = 1.0 / 120; // 120fps. 60fps using vsync = 1
float period = 4.0;

// Cureent frame
int frame = 0;

bool rotationLight = true;		// Rotate the lights

// shiniess time-varying value
GLfloat shininessCo = 20.0;
bool shininess = false;		
bool sDirection = true; // 0 to 128 : true, 128 to 0 : false;

// Sphere, cylinder
GLUquadricObj* sphere = NULL;
GLUquadricObj* cylinder = NULL;
GLUquadricObj* cone = NULL;

void reinitialize()
{
	frame = 0;

	lightOn[0] = true;	// Turn on only the point light
	lightOn[1] = false;
	lightOn[2] = false;

	thetaModel = 0;
	for (int i = 0; i < 3; i++)
		thetaLight[i] = 0;
}

void animate()
{
	frame += 1;

	// Rotation
	if (rotationLight)
	{
		for (int i = 0; i < 3; i++)
		{
			if (lightOn[i])thetaLight[i] += 4 / period;		// degree
		}
	}

	//shininess
	if (shininess)
	{
		if (sDirection)
			shininessCo += 2;
		else
			shininessCo -= 2;
		
		if (shininessCo == 128 || shininessCo == 0) // reach endpoint of shiniess
			sDirection = !sDirection;
	}
}

void setDot()
{
	p[0][0] = vec4(1.3, 1, 0, 1.0f); //start dot
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

	glColor3f(1, 1, 1);
	glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);

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
				glNormal3fv(value_ptr(nor[i][j]));
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
			glVertex3f(p[0][0].x, p[0][0].y, p[0][0].z);
			glVertex3f(p[0][17].x, p[0][17].y, p[0][17].z);
			glVertex3f(p[35][17].x, p[35][17].y, p[35][17].z);
			glVertex3f(p[35][0].x, p[35][0].y, p[35][0].z);
		}
		glEnd();
	}
}

void setNormalVector()
{
	int ys, zs; //temp ySweep, zSweep for last sweep
	vec3 v1, v2, c;
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
					glVertex3fv(value_ptr(p[i][j]));
					glVertex3f(p[i][j].x + nor[i][j].x, p[i][j].y + nor[i][j].y, p[i][j].z + nor[i][j].z);
				}
				glEnd();
			}
		}
	}
}

void init()
{
	// Animation system
	reinitialize();

	// Prepare quadric shapes
	sphere = gluNewQuadric();
	gluQuadricDrawStyle(sphere, GLU_FILL);
	gluQuadricNormals(sphere, GLU_SMOOTH);
	gluQuadricOrientation(sphere, GLU_OUTSIDE);
	gluQuadricTexture(sphere, GL_FALSE);

	cylinder = gluNewQuadric();
	gluQuadricDrawStyle(cylinder, GLU_FILL);
	gluQuadricNormals(cylinder, GLU_SMOOTH);
	gluQuadricOrientation(cylinder, GLU_OUTSIDE);
	gluQuadricTexture(cylinder, GL_FALSE);

	cone = gluNewQuadric();
	gluQuadricDrawStyle(cone, GLU_FILL);
	gluQuadricNormals(cone, GLU_SMOOTH);
	gluQuadricOrientation(cone, GLU_OUTSIDE);
	gluQuadricTexture(cone, GL_FALSE);

	// dot set
	setDot();

	// vector set
	setNormalVector();
}

//Draw a sphere using a GLU quadric
void drawSphere(float radius, int slices, int stacks)
{
	gluSphere(sphere, radius, slices, stacks);
}

// Draw a cylinder using a GLU quadric
void drawCylinder(float radius, float height, int slices, int stacks)
{
	gluCylinder(cylinder, radius, radius, height, slices, stacks);
}

// Draw a cone using a GLU quadric
void drawCone(float radius, float height, int slices, int stacks)
{
	gluCylinder(cone, 0, radius, height, slices, stacks);
}

// Compute the rotation axis and angle from a to b
//
// Axis is not normalized.
// theta is represented in degrees.
//
void computeRotation(const vec3& a, const vec3& b, float& theta, vec3& axis)
{
	axis = cross(a, b);
	float sinTheta = length(axis);
	float cosTheta = dot(a, b);
	theta = atan2(sinTheta, cosTheta) * 180 / M_PI;
}

// Material
void setupColoredMaterial(const vec3& color)
{
	// Material
	GLfloat mat_ambient[4] = { 0.1,0.1,0.1,1 };
	GLfloat mat_diffuse[4] = { color[0], color[1], color[2], 1 };
	GLfloat mat_specular[4] = { 0.8, 0.8, 0.8, 1 };

	glMaterialfv(GL_FRONT_AND_BACK, GL_AMBIENT, mat_ambient);
	glMaterialfv(GL_FRONT_AND_BACK, GL_DIFFUSE, mat_diffuse);
	glMaterialfv(GL_FRONT_AND_BACK, GL_SPECULAR, mat_specular);
	glMaterialf(GL_FRONT_AND_BACK, GL_SHININESS, shininessCo);
}

void setupLight(const vec4& p, int i)
{
	GLfloat ambient[4] = { 0.1, 0.1, 0.1, 1 };
	GLfloat diffuse[4] = { 0.7, 0.7, 0.7, 1 };
	GLfloat specular[4] = { 0.7, 0.7, 0.7, 1 };

	glLightfv(GL_LIGHT0 + i, GL_AMBIENT, ambient);
	glLightfv(GL_LIGHT0 + i, GL_DIFFUSE, diffuse);
	glLightfv(GL_LIGHT0 + i, GL_SPECULAR, specular);
	glLightfv(GL_LIGHT0 + i, GL_POSITION, value_ptr(p));
	// Attenuation for the point light
	if (i == 0 && attenuation)
	{
		glLightf(GL_LIGHT0 + i, GL_CONSTANT_ATTENUATION, 1.0);
		glLightf(GL_LIGHT0 + i, GL_LINEAR_ATTENUATION, 0.1);
		glLightf(GL_LIGHT0 + i, GL_QUADRATIC_ATTENUATION, 0.05);
	}
	else // Default value
	{
		glLightf(GL_LIGHT0 + i, GL_CONSTANT_ATTENUATION, 1.0);
		glLightf(GL_LIGHT0 + i, GL_LINEAR_ATTENUATION, 0.0);
		glLightf(GL_LIGHT0 + i, GL_QUADRATIC_ATTENUATION, 0.0);
	}

	if (i == 2) // Spot light
	{
		vec3 spotDirection = -vec3(p);
		glLightfv(GL_LIGHT0 + i, GL_SPOT_DIRECTION, value_ptr(spotDirection));
		glLightf(GL_LIGHT0 + i, GL_SPOT_CUTOFF, cutoffValue);	  // [0, 90]
		glLightf(GL_LIGHT0 + i, GL_SPOT_EXPONENT, exponentValue); // [0, 128]
	}
	else
	{ // Point and distant light.
		// 180 to turn off cutoff when it was used as a spot light.
		glLightf(GL_LIGHT0 + i, GL_SPOT_CUTOFF, 180); // uniform light distribution
	}

}

void drawArrow(const vec3& p, bool tailOnly)
{
	// make it possible to change a subset of material parameters
	glColorMaterial(GL_FRONT, GL_AMBIENT_AND_DIFFUSE);
	glEnable(GL_COLOR_MATERIAL);

	// Common material
	GLfloat mat_specular[4] = { 1,1,1,1 };
	glMaterialfv(GL_FRONT_AND_BACK, GL_SPECULAR, mat_specular);
	glMaterialf(GL_FRONT_AND_BACK, GL_SHININESS, shininessCo);

	// Transformation
	glPushMatrix();

	glTranslatef(p.x, p.y, p.z);

	if (!tailOnly)
	{
		float theta;
		vec3 axis;
		computeRotation(vec3(0, 0, 1), vec3(0, 1, 0) - vec3(p), theta, axis);
		glRotatef(theta, axis.x, axis.y, axis.z);
	}

	// Tail sphere
	float arrowTailRadius = 0.05;
	glColor3f(1, 0, 0); // ambient and diffuse
	drawSphere(arrowTailRadius, 16, 16);

	if (!tailOnly)
	{
		// Shaft cylinder
		float arrowShaftRadius = 0.02;
		float arrowShaftLength = 0.2;
		glColor3f(0, 1, 0);
		drawCylinder(arrowShaftRadius, arrowShaftLength, 16, 5);

		// Head cone
		float arrowheadHeight = 0.09;
		float arrowheadRadius = 0.06;
		glTranslatef(0, 0, arrowShaftLength + arrowheadHeight);
		glRotatef(180, 1, 0, 0);
		glColor3f(0, 0, 1); // ambient and diffuse
		drawCone(arrowheadRadius, arrowheadHeight, 16, 5);
	}
	glColor3f(1, 1, 1);
	glPopMatrix();

	// For convential material setting
	glDisable(GL_COLOR_MATERIAL);
}

void drawSpotLight(const vec3& p, float cutoff)
{
	glPushMatrix();

	glTranslatef(p.x, p.y, p.z);

	float theta;
	vec3 axis;
	computeRotation(vec3(0, 0, 1), vec3(0, 1, 0) - vec3(p), theta, axis);
	glRotatef(theta, axis.x, axis.y, axis.z);

	// Color
	setupColoredMaterial(vec3(1, 1, 1));

	// tan(cutoff) = r/h
	float h = 0.15;
	float r = h * tan(radians(cutoff));
	drawCone(r, h, 16, 5);

	// color
	setupColoredMaterial(vec3(1, 1, 1));

	// Apex
	float apexRadius = 0.06 * (0.5 + exponentValue / 128.0);
	drawSphere(apexRadius, 16, 16);

	glPopMatrix();

}

void render(GLFWwindow* window)
{
	// Background color
	glClearColor(bgColor[0], bgColor[1], bgColor[2], bgColor[3]);
	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

	// Modelview matrix
	glMatrixMode(GL_MODELVIEW);
	glLoadIdentity();

	// line, point size
	glLineWidth(3 * dpiScaling);
	glPointSize(2.0f);

	//view eye to zero
	gluLookAt(eye[0], eye[1], eye[2], center[0], center[1], center[2], up[0], up[1], up[2]);

	// Axes
	glDisable(GL_LIGHTING);
	drawAxes(AXIS_LENGTH, AXIS_LINE_WIDTH*dpiScaling);
	
	// Smooth shading
	glShadeModel(GL_SMOOTH);

	// Rotation of the light or 3x3 models
	vec3 axis(0, 1, 0);

	//  Lighting
	glEnable(GL_LIGHTING);

	// Set up the lights
	vec4 lightP[3];
	for (int i = 0; i < 3; i++)
	{
		// Just turn off the i-th light, if not lit
		if (!lightOn[i]) { glDisable(GL_LIGHT0 + i); continue; }

		// Turn on the i-th light
		glEnable(GL_LIGHT0 + i);

		// Dealing with the distant light
		lightP[i] = lightInitialP;
		if (i == 1) lightP[i].w = 0;

		// Lights rotate around the center of the world coordinate system
		mat4 R = rotate(mat4(1.0), radians(thetaLight[i]), axis);
		lightP[i] = R * lightP[i];

		// Set up the i-th light
		setupLight(lightP[i], i);
	}

	// Draw the geometries of the lights
	for (int i = 0; i < 3; i++)
	{
		if (!lightOn[i]) continue;

		if (i == 2) drawSpotLight(lightP[i], cutoffValue);
		else		drawArrow(lightP[i], i == 0);	// Tail only for a point light
	}

	// Draw
	switch (selection)
	{
		case 1: setFrame(1); drawNormalVector(toggle);	break;
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
			
				// Turn on/off the point, distant, spot light
			case GLFW_KEY_P: lightOn[0] = !lightOn[0]; break;
			case GLFW_KEY_D: lightOn[1] = !lightOn[1]; break;
			case GLFW_KEY_S: lightOn[2] = !lightOn[2]; break;

				// Example selection
			case GLFW_KEY_1: selection = 1; break;
			case GLFW_KEY_N: toggle = !toggle; break;

				// shiniess toggle
			case GLFW_KEY_T: shininess = !shininess; break;
		}
	}
}

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
	init();

	// ---Main loop---
	float previous = glfwGetTime();
	float elapsed = 0;
	while (!glfwWindowShouldClose(window))
	{
		glfwPollEvents();			// Events

		// Time passed during a single loop
		float now = glfwGetTime();
		float delta = now - previous;
		previous = now;

		// Time passed after the previous frame
		elapsed += delta;

		// Deal with the current frame

		if (elapsed > timeStep)
		{
			// Animate 1 frame
			if (!pause)	animate();

			elapsed = 0;	// reset the elapsed time
		}

		render(window);				// Draw one frame
		glfwSwapBuffers(window);	// Swap buffers
	}

	// Terminate the glfw system
	glfwDestroyWindow(window);
	glfwTerminate();

	return 0;
}
