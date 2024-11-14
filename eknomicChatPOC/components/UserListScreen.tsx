// UserListScreen.tsx

import React, { useEffect, useState } from 'react';
import { View, Text, FlatList, TouchableOpacity, StyleSheet } from 'react-native';
import database from '@react-native-firebase/database';
import { ref, onValue } from '@react-native-firebase/database';

const UserListScreen: React.FC<{ navigation: any }> = ({ navigation }) => {
  const [users, setUsers] = useState<any[]>([]);

  useEffect(() => {
    const usersRef = ref(database(), 'users'); // Assuming users are stored under 'users'

    const unsubscribe = onValue(usersRef, snapshot => {
      const data = snapshot.val();
      if (data) {
        const parsedUsers = Object.keys(data).map(key => ({
          id: key,
          name: data[key].name,
        }));
        setUsers(parsedUsers);
      }
    });

    // Cleanup subscription on unmount
    return () => unsubscribe();
  }, []);

  const handleUserPress = (user: any) => {
    navigation.navigate('Chat', {
      userId: user.id,
      username: user.name,
      groupId: 'defaultGroup', // Adjust this based on your logic
    });
  };

  return (
    <View style={styles.container}>
      <FlatList
        data={users}
        keyExtractor={(item) => item.id}
        renderItem={({ item }) => (
          <TouchableOpacity onPress={() => handleUserPress(item)}>
            <Text style={styles.userName}>{item.name}</Text>
          </TouchableOpacity>
        )}
      />
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    padding: 20,
  },
  userName: {
    padding: 15,
    borderBottomWidth: 1,
    borderBottomColor: '#ccc',
  },
});

export default UserListScreen;
