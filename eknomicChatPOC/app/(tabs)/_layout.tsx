import { FontAwesome } from "@expo/vector-icons";
import { BottomTabBar } from "@react-navigation/bottom-tabs";
import { BlurView } from "expo-blur";
import { Tabs } from "expo-router";
import React from "react";
import { Platform, View, Text } from "react-native";

export default function TabsLayout() {
  return (
    <Tabs
      initialRouteName="home"
      screenOptions={{
        tabBarStyle:
          Platform.OS === "ios"
            && {
                backgroundColor: "transparent",
              },
        headerShown: false,
      }}
      tabBar={(props) =>
        Platform.OS === "ios" ? (
          <BlurView
            style={{ position: "absolute", bottom: 0, left: 0, right: 0 }}
            intensity={95}
          >
            <BottomTabBar {...props} />
          </BlurView>
        ) : (
          <BottomTabBar {...props} />
        )
      }
    >
      <Tabs.Screen
        name="home"
        options={{
          href: "/home",
          title: "",
          tabBarIcon: ({ color }) => (
            <View
              style={{
                flexDirection: "column",
                alignItems: "center",
                marginTop: 17,
                backgroundColor: "transparent",
              }}
            >
              <TabBarIcon name="home" color={color} size={24} />
              <Text style={{ marginTop: 5, fontSize: 10, opacity: 0.5 }}>
                Home
              </Text>
            </View>
          ),
        }}
      />
      <Tabs.Screen
        name="chat"
        options={{
          title: "",
          headerShown: false,
          href: {
            pathname: "/chat",
          },
          tabBarIcon: ({ color }) => (
            <View
              style={{
                flexDirection: "column",
                alignItems: "center",
                marginTop: 17,
                backgroundColor: "transparent",
              }}
            >
              <TabBarIcon name="wechat" color={color} size={24} />
              <Text style={{ marginTop: 5, fontSize: 10, opacity: 0.5 }}>
                Chat
              </Text>
            </View>
          ),
        }}
      />
      <Tabs.Screen
        name="crt"
        options={{
          title: "",
          headerShown: false,
          href: {
            pathname: "/crt",
          },
          tabBarIcon: ({ color }) => (
            <View
              style={{
                flexDirection: "column",
                alignItems: "center",
                marginTop: 17,
                backgroundColor: "transparent",
              }}
            >
              <TabBarIcon name="wechat" color={color} size={24} />
              <Text style={{ marginTop: 5, fontSize: 10, opacity: 0.5 }}>
                Chat
              </Text>
            </View>
          ),
        }}
      />
    </Tabs>
  );
}

function TabBarIcon(props: {
  name: React.ComponentProps<typeof FontAwesome>["name"];
  color: string;
  size?: number;
}) {
  return (
    <FontAwesome
      size={props.size || 26}
      style={{ marginBottom: -3 }}
      {...props}
    />
  );
}
