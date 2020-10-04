#include "glSetup.h"

#include <vector>
#include <math.h>
#include <iostream>
using namespace std;

#include <Eigen/Dense>
using namespace Eigen;

void render(GLFWwindow* window);
void keyboard(GLFWwindow* window, int key, int code, int action, int mods);
void drag(GLFWwindow* window, double x, double y);
void mouse(GLFWwindow* window, int button, int action, int mods);
void addPoint();
void addEdge(int idx);
int selectPoint(Vector3f vec);
void initializeParticleSystem();
void solveODE();
void collisionHandling();
void setupLight();
void setupMaterial();
void drawSphere(float radius, const Vector3f& color, int N);
void init();
void quit();
void update();

Vector4f light(0.0, 0.0, 5.0, 1);

GLfloat bgColor[4] = { 1, 1, 1, 1 };

//play configuration
bool pause = true;
int frame = 0;

//cursor
Vector3f ee_goal;

//global coordinate frame
float AXIS_LENGTH = 3;
float AXIS_LINE_WIDTH = 2;

//sphere
GLUquadricObj* sphere = NULL;

//particles
vector<Vector3f> v;
vector<Vector3f> p;

//connectivity
float k0 = 1.0;

//edge Point e1, e2
vector<int> e1;
vector<int> e2;
vector<float> l;
vector<float> k;

vector<bool> constrained;

//Geometry and mass
float radius = 0.02;
float m = 0.01;
float dampValue = 0.1;
bool pointDamping = false;
bool dampedSpring = false;

//Time stepping
int N_SUBSTEPS = 1;
float h = 1.0 / 60.0 / N_SUBSTEPS;

//External force
float useGravity = true;
Vector3f gravity(0, -9.8, 0);

//Collision
float k_r = 0.75;
float epsilon = 1.0E-4;
vector<bool> contact;
vector<Vector3f> contactN;

const int nWalls = 4;
Vector3f wallP[nWalls];
Vector3f wallN[nWalls];

//key input
int condition = 0;

//select value
int selectedPoint;
int selectedCount = 0;
vector<bool> s;

//drag value
bool isDragged = false;

//Method
enum IntegrationMethod
{
	EULER = 1,
	MODIFIED_EULER,
};

IntegrationMethod intMethod = MODIFIED_EULER;

int main(int argc, char* argv[])
{
	perspectiveView = false;

	GLFWwindow* window = initializeOpenGL(argc, argv, bgColor);
	if (window == NULL) return -1;

	//Vertical sync for 60fps
	glfwSwapInterval(1);

	//Callbacks
	glfwSetKeyCallback(window, keyboard);
	glfwSetCursorPosCallback(window, drag);
	glfwSetMouseButtonCallback(window, mouse);

	//Depth test
	glDisable(GL_DEPTH_TEST);

	//Normal vectors are normalized after transformation
	glEnable(GL_NORMALIZE);

	//Back face culling
	glEnable(GL_CULL_FACE);
	glCullFace(GL_BACK);
	glFrontFace(GL_CCW);

	//Viewport and perspective setting
	reshape(window, windowW, windowH);

	//Initialization
	init();
	initializeParticleSystem();

	while (!glfwWindowShouldClose(window))
	{
		if (!pause) update();

		render(window);
		glfwSwapBuffers(window);
		glfwPollEvents();
	}
	quit();

	glfwDestroyWindow(window);
	glfwTerminate();

	return 0;
}

void rebuildSpringK()
{
	//Spring constants
	k.push_back(k0 / l[l.size()-1]);
}

void addPoint()
{
	p.push_back(ee_goal);
	v.push_back(Vector3f(0,0,0));

	//collision
	contact.push_back(false);
	contactN.push_back(Vector3f(0, 0, 0));
	constrained.push_back(false);

	//select initialize
	s.push_back(false);
}

int selectPoint(Vector3f vec)
{
	int index;
	float min = 100;

	//find nearest point
	for (int i = 0; i < p.size(); i++)
	{
		float x, y;
		x = (p[i][0] - vec[0]) * (p[i][0] - vec[0]);
		y = (p[i][1] - vec[1]) * (p[i][1] - vec[1]);
		if (min > x + y)
		{
			min = x + y;
			index = i;
		}
	}
	return index;
}

void addEdge(int idx)
{
	switch(selectedCount)
	{
		case 1:
		{
			e1.push_back(idx);
			break;
		}
		case 2:
		{
			e2.push_back(idx);
			l.push_back((p[e1[e1.size()-1]] - p[e2[e2.size()-1]]).norm());
			
			//init
			selectedCount = 0;
			s.clear();
			s.resize(p.size(), false);
			rebuildSpringK();
			break;
		}
		
	}
}

void initializeParticleSystem()
{
	//To generate the same random values at eact execution
	srand(0);

	//normal vector of the 4 surrounding walls
	wallN[0] = Vector3f(1.0, 0, 0); //l
	wallN[1] = Vector3f(-1.0, 0, 0); //r
	wallN[2] = Vector3f(0, 1.0, 0); //b
	wallN[3] = Vector3f(0, -1.0, 0); //top

	for (int i = 0; i < nWalls; i++)
		wallN[i].normalize();

	//collision handling
	collisionHandling();
}

void quit()
{
	//delete quadric shapes;
	gluDeleteQuadric(sphere);
}

void update()
{
	//solve ordinary differential equation
	for (int i = 0; i < N_SUBSTEPS; i++)
		solveODE();

	//Time increment
	frame++;
}

void solveODE()
{
	if(p.size() < 0)
		return;

	//Initialization
	vector<Vector3f> f;
	f.resize(p.size(), Vector3f(0, 0, 0));

	//ext force
	for (int i = 0; i < p.size(); i++)
	{
		//Gravity
		if (useGravity) f[i] += m * gravity;
	}

	//in force
	for (int i = 0; i < e2.size(); i++)
	{
		Vector3f v_i = p[e1[i]] - p[e2[i]];
		float L_i = v_i.norm();
		Vector3f f_i = k[i] * (L_i - l[i]) * v_i / L_i;

		f[e2[i]] += f_i;
		f[e1[i]] -= f_i;

		//add damping force
		if(pointDamping)
		{
			f[e2[i]] -= dampValue * v[e2[i]];
			f[e1[i]] -= dampValue * v[e1[i]];
		}
		if(dampedSpring)
		{
			Vector3f n = v_i / L_i; //nij
			Vector3f vel = v[e1[i]] - v[e2[i]]; //vij
			f[e2[i]] += dampValue * n.dot(vel) * n;
			f[e1[i]] -= dampValue * n.dot(vel) * n;
		}
	}

	for (int i = 0; i < p.size(); i++)
	{
		//constraint
		if (constrained[i]) continue;

		//contact force
		if (contact[i]) f[i] -= contactN[i].dot(f[i]) * contactN[i];

		switch (intMethod)
		{
		case EULER:
			p[i] += h * v[i];
			v[i] += h * f[i] / m;
			break;
		case MODIFIED_EULER:
			v[i] += h * f[i] / m;
			p[i] += h * v[i];
			break;
		}
	}

	//collision hadling
	collisionHandling();
}

void collisionHandling()
{
	//Points of the 4 surrounding walls: it can be changed.
	wallP[0] = Vector3f(-1.0 * aspect, 0, 0); //l
	wallP[1] = Vector3f(1.0 * aspect, 0, 0); //r
	wallP[2] = Vector3f(0, -1.0, 0); //b
	wallP[3] = Vector3f(0, 1.0, 0); //top

	//collision wrt the walls
	for (int i = 0; i < p.size(); i++)
	{
		contact[i] = false;
		for (int j = 0; j < nWalls; j++)
		{
			float d_N = wallN[j].dot(p[i] - wallP[j]);
			if (d_N < radius)
			{
				//position correction
				p[i] += (radius - d_N) * wallN[j];

				//normal velocity
				float v_N = wallN[j].dot(v[i]);

				if (fabs(v_N) < epsilon)
				{
					contact[i] = true;
					contactN[i] = wallN[j];
				}
				else if (v_N < 0)
				{
					v[i] -= (1 + k_r) * v_N * wallN[j];
				}
			}
		}
	}
}

void render(GLFWwindow* window)
{
	glClearColor(bgColor[0], bgColor[1], bgColor[2], bgColor[3]);
	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

	glMatrixMode(GL_MODELVIEW);
	glLoadIdentity();

	//setting
	setupLight();
	setupMaterial();

	//Particle
	for (int i = 0; i < p.size(); i++)
	{
		glPushMatrix();
		glTranslatef(p[i][0], p[i][1], p[i][2]);
		if (constrained[i])
			drawSphere(radius, Vector3f(1, 1, 0), 20);
		else if(s[i])
			drawSphere(radius, Vector3f(1, 0, 0), 20);
		else
			drawSphere(radius, Vector3f(0, 1, 0), 20);
		glPopMatrix();	
	}

	//Edge
	glLineWidth(7 * dpiScaling);
	glColor3f(0, 0, 1);
	glBegin(GL_LINES);
	for (int i = 0; i < l.size(); i++)
	{
		glVertex3fv(p[e1[i]].data());
		glVertex3fv(p[e2[i]].data());
	}
	glEnd();
}

void keyboard(GLFWwindow* window, int key, int scancode, int action, int mods)
{
	if (action == GLFW_PRESS || action == GLFW_REPEAT)
	{
		switch (key)
		{
			//Quit
		case GLFW_KEY_Q:
		case GLFW_KEY_ESCAPE: glfwSetWindowShouldClose(window, GL_TRUE); break;
		
			//Control
		case GLFW_KEY_SPACE: pause = !pause; break;
		case GLFW_KEY_A: condition = 1; cout << "Add Point" << endl; break;
		case GLFW_KEY_T: condition = 2; cout << "Attach Point" << endl; break;
		case GLFW_KEY_N: condition = 3; cout << "nail Point" << endl; break;
		case GLFW_KEY_D: condition = 4; cout << "drag Point" << endl; break;
		case GLFW_KEY_G: useGravity = !useGravity; break;
		case GLFW_KEY_E: intMethod = EULER; cout << "euler" << endl; break;
		case GLFW_KEY_M: intMethod = MODIFIED_EULER; cout << "Modified-euler" << endl; break;
		case GLFW_KEY_P: pointDamping = !pointDamping; cout << "point damping on/off" << endl; break;
		case GLFW_KEY_O: dampedSpring = !dampedSpring; cout << "damped spring on/off" << endl; break;

			//Spring constants
		case GLFW_KEY_UP: k0 = min(k0 + 0.1, 10.0); rebuildSpringK(); cout << "k : " << k0 << endl; break;
		case GLFW_KEY_DOWN: k0 = max(k0 - 0.1, 0.1); rebuildSpringK(); cout << "k : " << k0 << endl; break;
		case GLFW_KEY_RIGHT: N_SUBSTEPS = min(N_SUBSTEPS + 5.0, 1000.0); h = 1.0 / 60.0 / N_SUBSTEPS; cout << "h : " << h << endl; break;
		case GLFW_KEY_LEFT: N_SUBSTEPS = max(N_SUBSTEPS - 5.0, 1.0); h = 1.0 / 60.0 / N_SUBSTEPS; cout << "h : " << h << endl; break;

			//constraints remove
		case GLFW_KEY_R: constrained.clear(); constrained.resize(p.size(), false); cout << "constrait removed" << endl; break;
		}
	}
}

void init()
{
	sphere = gluNewQuadric();
	gluQuadricDrawStyle(sphere, GLU_FILL);
	gluQuadricNormals(sphere, GLU_SMOOTH);
	gluQuadricOrientation(sphere, GLU_OUTSIDE);
	gluQuadricTexture(sphere, GL_FALSE);
}

void setupLight()
{
	glEnable(GL_LIGHTING);
	glEnable(GL_LIGHT0);

	GLfloat ambient[4] = { 0.1, 0.1, 0.1, 1 };
	GLfloat diffuse[4] = { 1.0, 1.0, 1.0, 1 };
	GLfloat specular[4] = { 1.0, 1.0, 1.0, 1 };

	glLightfv(GL_LIGHT0, GL_AMBIENT, ambient);
	glLightfv(GL_LIGHT0, GL_DIFFUSE, diffuse);
	glLightfv(GL_LIGHT0, GL_SPECULAR, specular);
	glLightfv(GL_LIGHT0, GL_POSITION, light.data());
}

void setupMaterial()
{
	GLfloat mat_ambient[4] = { 0.1, 0.1, 0.1, 1 };
	GLfloat mat_specular[4] = { 0.5, 0.5, 0.5, 1 };
	GLfloat mat_shininess = 128;

	glMaterialfv(GL_FRONT_AND_BACK, GL_AMBIENT, mat_ambient);
	glMaterialfv(GL_FRONT_AND_BACK, GL_SPECULAR, mat_specular);
	glMaterialf(GL_FRONT_AND_BACK, GL_SHININESS, mat_shininess);
}

void setDiffuseColor(const Vector3f& color)
{
	GLfloat mat_diffuse[4] = { color[0], color[1], color[2], 1 };
	glMaterialfv(GL_FRONT_AND_BACK, GL_DIFFUSE, mat_diffuse);
}

void drawSphere(float radius, const Vector3f& color, int N)
{
	setDiffuseColor(color);
	gluSphere(sphere, radius, N, N);
}

void drag(GLFWwindow* window, double x, double y)
{
	if (isDragged)
	{
		float aspect = (float)screenW / screenH;
		x = 2.0 * (x / screenW - 0.5) * aspect;
		y = -2.0 * (y / screenH - 0.5);
		ee_goal[0] += x - ee_goal[0];
		ee_goal[1] += y - ee_goal[1];
		Vector3f newPosition = Vector3f(ee_goal[0], ee_goal[1], .0);
		p[selectedPoint] = newPosition;
	}
}

void mouse(GLFWwindow* window, int button, int action, int mods)
{
	if (button == GLFW_MOUSE_BUTTON_LEFT)
	{
		if (action == GLFW_PRESS)
		{
			//find cursor
			double xpos, ypos;
			glfwGetCursorPos(window, &xpos, &ypos);
			ee_goal = Vector3f(xpos, ypos, 0);

			float aspect = (float)screenW / screenH;
			ee_goal[0] = 2.0 * (ee_goal[0] / screenW - 0.5) * aspect;
			ee_goal[1] = -2.0 * (ee_goal[1] / screenH - 0.5);

			switch (condition)
			{
			//a key
			case 1:
			{
				addPoint();
				break;
			}
			//t key
			case 2:
			{
				//selected check
				int idx = selectPoint(ee_goal);
				s[idx] = true;
				selectedCount++;

				//add edge
				addEdge(idx);
				break;
			}
			//n key
			case 3:
			{
				int idx = selectPoint(ee_goal);
				constrained[idx] = true;

				break;
			}
			//d key
			case 4:
			{
				isDragged = true;
				selectedPoint = selectPoint(ee_goal);
				break;
			}

			}
		}
		if (action == GLFW_RELEASE)
			isDragged = false;
	}
}